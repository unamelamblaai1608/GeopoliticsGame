using Geopolitics.Core.Models;

namespace Geopolitics.Core.Engines;

public class NpcArchetype
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Personality { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class ConquestResolution
{
    public int ConquerBonus { get; set; }
    public int TroopsTransferred { get; set; }
    public bool DefenderEliminated { get; set; }
    public bool GameWon { get; set; }
}

public class CombatStatusResult
{
    public string Status { get; set; } = "ongoing"; // conquered, attacker_defeated, ongoing
    public int ConquerBonus { get; set; }
}

public static class GameEngine
{
    public const string PlayerColor = "#3b82f6";

    public static readonly List<NpcArchetype> Archetypes = new()
    {
        new() { Name = "General Vance", Title = "The Warmonger", Personality = "aggressive", Color = "#ef4444" },
        new() { Name = "Strategist Thorne", Title = "The Tactician", Personality = "calculating", Color = "#8b5cf6" },
        new() { Name = "Commander Chen", Title = "The Iron Fortress", Personality = "defensive", Color = "#10b981" },
        new() { Name = "Director Al-Mansoor", Title = "The Opportunist", Personality = "opportunist", Color = "#f59e0b" },
        new() { Name = "Admiral Moreau", Title = "The Expansionist", Personality = "aggressive", Color = "#ec4899" },
        new() { Name = "Marshal Petrov", Title = "The Hegemon", Personality = "calculating", Color = "#06b6d4" },
        new() { Name = "Governor Silva", Title = "The Consolidator", Personality = "defensive", Color = "#84cc16" },
        new() { Name = "Chancellor Weber", Title = "The Pragmatist", Personality = "opportunist", Color = "#6366f1" },
        new() { Name = "Emir Qasim", Title = "The Sovereign", Personality = "calculating", Color = "#d946ef" }
    };

    [ThreadStatic]
    private static Random? _random;
    public static Random Rng => _random ??= new Random();

    public static int RollDie() => Rng.Next(1, 7);

    public static List<T> Shuffle<T>(IEnumerable<T> source)
    {
        var list = source.ToList();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    public static GameState CreateNewGame(string userId, string username, string? selectedCountryId = "za", string? title = "Global Conquest")
    {
        var targetId = string.IsNullOrWhiteSpace(selectedCountryId) ? "za" : selectedCountryId;
        var userCountry = CountriesData.AllCountries.FirstOrDefault(c => c.Id == targetId)
                          ?? CountriesData.AllCountries.First(c => c.Id == "za");

        var availableOthers = CountriesData.AllCountries.Where(c => c.Id != userCountry.Id).ToList();
        var selectedOthers = Shuffle(availableOthers).Take(9).ToList();
        var allGameCountries = new List<Country> { userCountry.Clone() };
        allGameCountries.AddRange(selectedOthers.Select(c => c.Clone()));

        var humanPlayer = new Player
        {
            Id = $"player_{userId}",
            UserId = userId,
            Name = username,
            IsHuman = true,
            StartingCountryId = userCountry.Id,
            Color = PlayerColor,
            Personality = "human",
            Title = "Supreme Commander",
            Eliminated = false,
            Stats = new PlayerBattleStats()
        };

        var npcPlayers = selectedOthers.Select((c, idx) =>
        {
            var arch = Archetypes[idx % Archetypes.Count];
            return new Player
            {
                Id = $"npc_{idx + 1}_{c.Id}",
                Name = $"{arch.Name} ({c.Name})",
                IsHuman = false,
                StartingCountryId = c.Id,
                Color = arch.Color,
                Personality = arch.Personality,
                Title = arch.Title,
                Eliminated = false,
                Stats = new PlayerBattleStats()
            };
        }).ToList();

        var players = new List<Player> { humanPlayer };
        players.AddRange(npcPlayers);

        var countries = new Dictionary<string, Country>();
        foreach (var c in allGameCountries)
        {
            var owner = players.FirstOrDefault(p => p.StartingCountryId == c.Id);
            c.OwnerId = owner?.Id;
            c.StartingOwnerId = owner?.Id;
            countries[c.Id] = c;
        }

        var turnOrder = Shuffle(players.Select(p => p.Id));
        var currentTurnIndex = 0;
        var currentTurnPlayerId = turnOrder[currentTurnIndex];
        var starterName = players.First(p => p.Id == currentTurnPlayerId).Name;

        return new GameState
        {
            Id = Guid.NewGuid().ToString(),
            Title = string.IsNullOrWhiteSpace(title) ? "Global Hegemony" : title,
            OwnerUserId = userId,
            OwnerUsername = username,
            CreatedAt = DateTime.UtcNow.ToString("o"),
            Status = "active",
            WinnerId = null,
            WinnerName = null,
            RoundNumber = 1,
            TurnNumber = 1,
            TurnOrder = turnOrder,
            CurrentTurnIndex = currentTurnIndex,
            CurrentTurnPlayerId = currentTurnPlayerId,
            Players = players,
            Countries = countries,
            PendingReinforcements = 0,
            HasAttackedThisTurn = false,
            ActiveBattle = null,
            BattleHistory = new(),
            NpcDecisionLog = new(),
            RecentCommentary = new()
            {
                new CommentaryEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow.ToString("o"),
                    Text = $"War Room initialized! 10 nations enter the arena. {starterName} takes the first turn in Round 1.",
                    Type = "headline"
                }
            }
        };
    }

    public static int CalculatePlayerReinforcements(GameState game, string playerId)
    {
        if (game.RoundNumber < 2) return 0;
        var ownedCount = game.Countries.Values.Count(c => c.OwnerId == playerId);
        return ownedCount * 2;
    }

    public static GameState AdvanceTurn(GameState game)
    {
        var activeFactions = game.Players.Where(p => !p.Eliminated).ToList();
        if (activeFactions.Count == 1)
        {
            game.Status = "completed";
            game.WinnerId = activeFactions[0].Id;
            game.WinnerName = activeFactions[0].Name;
            game.ActiveBattle = null;
            return game;
        }

        int nextIndex = game.CurrentTurnIndex;
        int checkedCount = 0;
        bool wrapped = false;

        do
        {
            nextIndex = (nextIndex + 1) % game.TurnOrder.Count;
            if (nextIndex == 0)
            {
                wrapped = true;
            }
            checkedCount++;
            var nextPlayerId = game.TurnOrder[nextIndex];
            var player = game.Players.FirstOrDefault(p => p.Id == nextPlayerId);
            if (player != null && !player.Eliminated)
            {
                break;
            }
        } while (checkedCount <= game.TurnOrder.Count * 2);

        if (wrapped)
        {
            game.RoundNumber += 1;
        }

        game.CurrentTurnIndex = nextIndex;
        game.CurrentTurnPlayerId = game.TurnOrder[nextIndex];
        game.TurnNumber += 1;
        game.HasAttackedThisTurn = false;
        game.ActiveBattle = null;

        game.PendingReinforcements = CalculatePlayerReinforcements(game, game.CurrentTurnPlayerId);
        return game;
    }

    public static ClashResult ExecuteClashRound(Country attackerCountry, Country defenderCountry)
    {
        int attackerMax = attackerCountry.Soldiers - 1;
        int defenderMax = defenderCountry.Soldiers;

        if (attackerMax <= 0 || defenderMax <= 0)
        {
            throw new InvalidOperationException("Insufficient soldiers to execute battle clash");
        }

        int diceCount = Math.Min(attackerMax, defenderMax);
        var attackerDice = new List<int>();
        var defenderDice = new List<int>();

        for (int i = 0; i < diceCount; i++)
        {
            attackerDice.Add(RollDie());
            defenderDice.Add(RollDie());
        }

        int attackerLosses = 0;
        int defenderLosses = 0;
        var pairings = new List<DicePairing>();

        for (int i = 0; i < diceCount; i++)
        {
            int a = attackerDice[i];
            int d = defenderDice[i];
            string outcome = "tie";

            if (a > d)
            {
                outcome = "attacker_won";
                defenderLosses++;
            }
            else if (d > a)
            {
                outcome = "defender_won";
                attackerLosses++;
            }

            pairings.Add(new DicePairing
            {
                AttackerDie = a,
                DefenderDie = d,
                Outcome = outcome
            });
        }

        attackerCountry.Soldiers -= attackerLosses;
        defenderCountry.Soldiers -= defenderLosses;

        return new ClashResult
        {
            DiceCount = diceCount,
            AttackerDice = attackerDice,
            DefenderDice = defenderDice,
            Pairings = pairings,
            AttackerLosses = attackerLosses,
            DefenderLosses = defenderLosses,
            AttackerRemaining = attackerCountry.Soldiers,
            DefenderRemaining = defenderCountry.Soldiers
        };
    }

    public static CombatStatusResult EvaluateCombatStatus(Country attackerCountry, Country defenderCountry)
    {
        if (defenderCountry.Soldiers <= 0)
        {
            int bonus = Math.Max(2, (int)Math.Floor(defenderCountry.InitialSoldiers / 2.0));
            return new CombatStatusResult
            {
                Status = "conquered",
                ConquerBonus = bonus
            };
        }

        if (attackerCountry.Soldiers <= 1)
        {
            return new CombatStatusResult
            {
                Status = "attacker_defeated"
            };
        }

        return new CombatStatusResult
        {
            Status = "ongoing"
        };
    }

    public static ConquestResolution ResolveConquest(
        GameState game,
        Player attackerPlayer,
        Player? defenderPlayer,
        Country attackerCountry,
        Country defenderCountry,
        int transferCount = 0)
    {
        int bonus = Math.Max(2, (int)Math.Floor(defenderCountry.InitialSoldiers / 2.0));
        string prevOwnerId = defenderCountry.OwnerId ?? string.Empty;

        defenderCountry.OwnerId = attackerPlayer.Id;
        defenderCountry.Soldiers = bonus;

        int maxCanTransfer = Math.Max(0, attackerCountry.Soldiers - 1);
        int troopsToTransfer = Math.Clamp(transferCount, 0, maxCanTransfer);

        if (troopsToTransfer > 0)
        {
            attackerCountry.Soldiers -= troopsToTransfer;
            defenderCountry.Soldiers += troopsToTransfer;
        }

        attackerPlayer.Stats.CountriesConquered++;

        bool defenderEliminated = false;
        if (!string.IsNullOrEmpty(prevOwnerId))
        {
            var remainingOwned = game.Countries.Values.Count(c => c.OwnerId == prevOwnerId);
            if (remainingOwned == 0)
            {
                defenderEliminated = true;
                if (defenderPlayer != null)
                {
                    defenderPlayer.Eliminated = true;
                }
            }
        }

        bool allOwned = game.Countries.Values.All(c => c.OwnerId == attackerPlayer.Id);
        if (allOwned)
        {
            game.Status = "completed";
            game.WinnerId = attackerPlayer.Id;
            game.WinnerName = attackerPlayer.Name;
        }

        return new ConquestResolution
        {
            ConquerBonus = bonus,
            TroopsTransferred = troopsToTransfer,
            DefenderEliminated = defenderEliminated,
            GameWon = allOwned
        };
    }
}

