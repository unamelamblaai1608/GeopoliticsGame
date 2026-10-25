using Geopolitics.Core.Models;

namespace Geopolitics.Core.Engines;

public class ReinforcementPlan
{
    public Dictionary<string, int> Allocations { get; set; } = new();
    public string Justification { get; set; } = string.Empty;
}

public class AttackTargetPlan
{
    public string FromCountryId { get; set; } = string.Empty;
    public string ToCountryId { get; set; } = string.Empty;
    public double WinProb { get; set; }
    public int ConquerReward { get; set; }
    public string Justification { get; set; } = string.Empty;
}

public class TroopTransferPlan
{
    public string FromCountryId { get; set; } = string.Empty;
    public string ToCountryId { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Justification { get; set; } = string.Empty;
}

public class StopAttackDecision
{
    public bool Stop { get; set; }
    public string Justification { get; set; } = string.Empty;
}

public class NpcTurnSimulationResult
{
    public List<NpcStep> LogSteps { get; set; } = new();
    public ActiveBattle? BattleResult { get; set; }
}

public static class NpcEngine
{
    public static double EstimateWinProbability(int attackerSoldiers, int defenderSoldiers, int simulations = 80)
    {
        if (attackerSoldiers <= 1) return 0;
        if (defenderSoldiers <= 0) return 1;

        int wins = 0;
        for (int s = 0; s < simulations; s++)
        {
            int a = attackerSoldiers;
            int d = defenderSoldiers;
            while (a > 1 && d > 0)
            {
                int diceCount = Math.Min(a - 1, d);
                for (int i = 0; i < diceCount; i++)
                {
                    int aRoll = GameEngine.RollDie();
                    int dRoll = GameEngine.RollDie();
                    if (aRoll > dRoll) d--;
                    else if (dRoll > aRoll) a--;
                }
            }
            if (d <= 0) wins++;
        }
        return (double)wins / simulations;
    }

    public static ReinforcementPlan PlanReinforcements(GameState game, Player npcPlayer, int totalReinforcements)
    {
        var owned = game.Countries.Values.Where(c => c.OwnerId == npcPlayer.Id).ToList();
        if (owned.Count == 0 || totalReinforcements <= 0)
        {
            return new ReinforcementPlan { Justification = "No reinforcements available." };
        }

        var enemies = game.Countries.Values.Where(c => c.OwnerId != npcPlayer.Id).ToList();

        var scored = owned.Select(country =>
        {
            double attackPotential = 0;
            foreach (var enemy in enemies)
            {
                double winOdds = EstimateWinProbability(country.Soldiers + totalReinforcements, enemy.Soldiers, 30);
                if (winOdds > attackPotential) attackPotential = winOdds;
            }

            double personalityWeight = npcPlayer.Personality switch
            {
                "aggressive" => country.Soldiers * 0.8 + attackPotential * 10,
                "defensive" => 10 - country.Soldiers,
                _ => country.Soldiers * 0.5 + attackPotential * 5
            };

            return new { Country = country, Score = personalityWeight };
        }).OrderByDescending(x => x.Score).ToList();

        var allocations = owned.ToDictionary(c => c.Id, _ => 0);
        int remaining = totalReinforcements;
        var primary = scored[0].Country;

        if (npcPlayer.Personality == "aggressive")
        {
            allocations[primary.Id] = remaining;
            remaining = 0;
        }
        else
        {
            int mainShare = (int)Math.Ceiling(remaining * 0.7);
            allocations[primary.Id] += mainShare;
            remaining -= mainShare;

            if (remaining > 0 && scored.Count > 1)
            {
                allocations[scored[1].Country.Id] += remaining;
                remaining = 0;
            }
            else
            {
                allocations[primary.Id] += remaining;
                remaining = 0;
            }
        }

        var targetNames = allocations.Where(kvp => kvp.Value > 0)
            .Select(kvp => $"+{kvp.Value} to {game.Countries[kvp.Key].Name}");

        string justification = $"Reinforced strategic positions ({string.Join(", ", targetNames)}) to assemble an optimal assault vector under {npcPlayer.Title} doctrine.";

        return new ReinforcementPlan
        {
            Allocations = allocations,
            Justification = justification
        };
    }

    public static AttackTargetPlan? SelectAttackTarget(GameState game, Player npcPlayer)
    {
        var owned = game.Countries.Values.Where(c => c.OwnerId == npcPlayer.Id && c.Soldiers >= 2).ToList();
        var enemies = game.Countries.Values.Where(c => c.OwnerId != npcPlayer.Id).ToList();

        if (owned.Count == 0 || enemies.Count == 0) return null;

        double minThreshold = npcPlayer.Personality switch
        {
            "aggressive" => 0.45,
            "opportunist" => 0.70,
            "defensive" => 0.75,
            _ => 0.60
        };

        var attackOptions = new List<(Country From, Country To, double WinProb, int ConquerReward, double Score)>();

        foreach (var from in owned)
        {
            foreach (var to in enemies)
            {
                double winProb = EstimateWinProbability(from.Soldiers, to.Soldiers, 50);
                int reward = Math.Max(2, (int)Math.Floor(to.InitialSoldiers / 2.0));

                double expectedDividend = winProb * reward;
                double riskPenalty = (1 - winProb) * (from.Soldiers - 1);
                double score = (expectedDividend * 2) - riskPenalty + (winProb * 10);

                if (winProb >= minThreshold)
                {
                    attackOptions.Add((from, to, winProb, reward, score));
                }
            }
        }

        if (attackOptions.Count == 0) return null;

        var best = attackOptions.OrderByDescending(x => x.Score).First();
        string justification = $"Targeted {best.To.Name} ({best.To.Soldiers} troops) with {best.From.Name} ({best.From.Soldiers} troops). Win projection: {(best.WinProb * 100):F1}% with potential +{best.ConquerReward} soldier conquest bounty.";

        return new AttackTargetPlan
        {
            FromCountryId = best.From.Id,
            ToCountryId = best.To.Id,
            WinProb = best.WinProb,
            ConquerReward = best.ConquerReward,
            Justification = justification
        };
    }

    public static StopAttackDecision ShouldStopOngoingAttack(Player npcPlayer, Country attackerCountry, Country defenderCountry, int roundsFought)
    {
        if (attackerCountry.Soldiers <= 1) return new StopAttackDecision { Stop = true };
        if (defenderCountry.Soldiers <= 0) return new StopAttackDecision { Stop = false };

        double stopThreshold = npcPlayer.Personality switch
        {
            "aggressive" => 0.25,
            "defensive" => 0.55,
            "opportunist" => 0.60,
            _ => 0.40
        };

        double currentWinOdds = EstimateWinProbability(attackerCountry.Soldiers, defenderCountry.Soldiers, 40);

        if (currentWinOdds < stopThreshold)
        {
            return new StopAttackDecision
            {
                Stop = true,
                Justification = $"Tactical retreat ordered: Win probability declined to {(currentWinOdds * 100):F0}%. Preserving remaining {attackerCountry.Soldiers} soldiers at base."
            };
        }

        return new StopAttackDecision { Stop = false };
    }

    public static TroopTransferPlan? PlanTroopTransfers(GameState game, Player npcPlayer)
    {
        var owned = game.Countries.Values.Where(c => c.OwnerId == npcPlayer.Id).ToList();
        if (owned.Count < 2) return null;

        var oversupplied = owned.Where(c => c.Soldiers > 3).OrderByDescending(c => c.Soldiers).ToList();
        if (oversupplied.Count == 0) return null;

        var source = oversupplied[0];
        var needy = owned.Where(c => c.Id != source.Id).OrderBy(c => c.Soldiers).ToList();
        if (needy.Count == 0) return null;

        var target = needy[0];
        int maxTransfer = source.Soldiers - 1;
        int transferCount = Math.Min(maxTransfer / 2, 3);
        if (transferCount <= 0) return null;

        return new TroopTransferPlan
        {
            FromCountryId = source.Id,
            ToCountryId = target.Id,
            Count = transferCount,
            Justification = $"Redeployed {transferCount} soldiers from {source.Name} to reinforce garrison in {target.Name}."
        };
    }

    public static NpcTurnSimulationResult SimulateNpcTurn(GameState game, Player npcPlayer)
    {
        var logSteps = new List<NpcStep>();

        // Step 1: Reinforcements
        if (game.PendingReinforcements > 0)
        {
            var plan = PlanReinforcements(game, npcPlayer, game.PendingReinforcements);
            foreach (var (cId, count) in plan.Allocations)
            {
                if (count > 0 && game.Countries.TryGetValue(cId, out var country))
                {
                    country.Soldiers += count;
                }
            }
            game.PendingReinforcements = 0;
            logSteps.Add(new NpcStep { Action = "reinforce", Text = plan.Justification });
        }

        // Step 2: Troop transfer
        var transferPlan = PlanTroopTransfers(game, npcPlayer);
        if (transferPlan != null)
        {
            if (game.Countries.TryGetValue(transferPlan.FromCountryId, out var fromC) &&
                game.Countries.TryGetValue(transferPlan.ToCountryId, out var toC) &&
                fromC.Soldiers - transferPlan.Count >= 1)
            {
                fromC.Soldiers -= transferPlan.Count;
                toC.Soldiers += transferPlan.Count;
                logSteps.Add(new NpcStep { Action = "transfer", Text = transferPlan.Justification });
            }
        }

        // Step 3: Attack or Skip
        var attackPlan = SelectAttackTarget(game, npcPlayer);
        ActiveBattle? battleResult = null;

        if (attackPlan == null)
        {
            logSteps.Add(new NpcStep
            {
                Action = "skip",
                Text = "Tactical skip chosen: Current geopolitical landscape offers no high-confidence assault corridors."
            });
        }
        else
        {
            logSteps.Add(new NpcStep { Action = "attack_declare", Text = attackPlan.Justification });

            var fromCountry = game.Countries[attackPlan.FromCountryId];
            var toCountry = game.Countries[attackPlan.ToCountryId];
            var defenderPlayer = game.Players.FirstOrDefault(p => p.Id == toCountry.OwnerId);

            var battleRecord = new ActiveBattle
            {
                BattleId = $"battle_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid().ToString("N")[..4]}",
                RoundNumber = game.RoundNumber,
                TurnNumber = game.TurnNumber,
                AttackerPlayerId = npcPlayer.Id,
                AttackerPlayerName = npcPlayer.Name,
                DefenderPlayerId = defenderPlayer?.Id,
                DefenderPlayerName = defenderPlayer?.Name ?? "Unknown",
                AttackerCountryId = fromCountry.Id,
                AttackerCountryName = fromCountry.Name,
                DefenderCountryId = toCountry.Id,
                DefenderCountryName = toCountry.Name,
                AttackerStartingSoldiers = fromCountry.Soldiers,
                DefenderStartingSoldiers = toCountry.Soldiers,
                Rounds = new(),
                Status = "ongoing",
                Outcome = null,
                Conquered = false
            };

            int roundsFought = 0;
            while (true)
            {
                roundsFought++;
                var clash = GameEngine.ExecuteClashRound(fromCountry, toCountry);
                battleRecord.Rounds.Add(clash);

                var status = GameEngine.EvaluateCombatStatus(fromCountry, toCountry);
                if (status.Status == "conquered")
                {
                    int transferBonus = Math.Max(0, (fromCountry.Soldiers - 1) / 2);
                    var conquest = GameEngine.ResolveConquest(game, npcPlayer, defenderPlayer, fromCountry, toCountry, transferBonus);
                    battleRecord.Status = "conquered";
                    battleRecord.Outcome = $"{npcPlayer.Name} conquered {toCountry.Name}!";
                    battleRecord.Conquered = true;
                    battleRecord.ConquerBonus = conquest.ConquerBonus;
                    battleRecord.DefenderEliminated = conquest.DefenderEliminated;

                    logSteps.Add(new NpcStep
                    {
                        Action = "conquest",
                        Text = $"VICTORY! Conquered {toCountry.Name}! Received +{conquest.ConquerBonus} bonus soldiers and secured strategic dominance."
                    });
                    break;
                }

                if (status.Status == "attacker_defeated")
                {
                    battleRecord.Status = "attacker_defeated";
                    battleRecord.Outcome = $"Assault on {toCountry.Name} failed! Attacker depleted to 1 soldier.";
                    logSteps.Add(new NpcStep
                    {
                        Action = "defeat",
                        Text = $"Assault stalled: Expeditionary force repelled at {toCountry.Name}. Garrison reduced to 1 soldier."
                    });
                    break;
                }

                var stopCheck = ShouldStopOngoingAttack(npcPlayer, fromCountry, toCountry, roundsFought);
                if (stopCheck.Stop)
                {
                    battleRecord.Status = "retreated";
                    battleRecord.Outcome = $"{npcPlayer.Name} broke off the siege of {toCountry.Name}.";
                    logSteps.Add(new NpcStep
                    {
                        Action = "retreat",
                        Text = stopCheck.Justification
                    });
                    break;
                }
            }

            battleResult = battleRecord;
            game.BattleHistory.Add(battleRecord);
        }

        game.NpcDecisionLog.Add(new NpcDecisionLogEntry
        {
            TurnNumber = game.TurnNumber,
            RoundNumber = game.RoundNumber,
            PlayerId = npcPlayer.Id,
            PlayerName = npcPlayer.Name,
            Steps = logSteps,
            Timestamp = DateTime.UtcNow.ToString("o")
        });

        return new NpcTurnSimulationResult
        {
            LogSteps = logSteps,
            BattleResult = battleResult
        };
    }
}

