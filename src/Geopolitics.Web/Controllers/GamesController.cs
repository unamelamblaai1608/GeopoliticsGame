using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Geopolitics.Core.Services;
using Geopolitics.Core.Models;
using Geopolitics.Core.Engines;
using Geopolitics.Core.Interfaces;

namespace Geopolitics.Web.Controllers;

/// <summary>
/// Games API controller — manages game CRUD, turn actions, combat,
/// NPC simulation, and game state queries with search/filter/pagination.
/// </summary>
[ApiController]
[Route("api/games")]
[Authorize]
public class GamesController : ControllerBase
{
    private readonly IDatabaseService _db;
    private readonly IGameEngine _gameEngine;
    private readonly INpcEngine _npcEngine;
    private readonly ICommentaryEngine _commentary;
    private readonly IAnalyticsEngine _analytics;

    public GamesController(
        IDatabaseService db,
        IGameEngine gameEngine,
        INpcEngine npcEngine,
        ICommentaryEngine commentary,
        IAnalyticsEngine analytics)
    {
        _db = db;
        _gameEngine = gameEngine;
        _npcEngine = npcEngine;
        _commentary = commentary;
        _analytics = analytics;
    }

    private string? GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    private string? GetUsername() => User.FindFirst("username")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

    // GET /api/games?status=active&search=my+game&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetGames(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var games = await _db.GetGamesByOwnerAsync(userId);

        // Filter by status (active / completed)
        if (!string.IsNullOrWhiteSpace(status))
            games = games.Where(g => g.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

        // Search by title substring
        if (!string.IsNullOrWhiteSpace(search))
            games = games.Where(g => g.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        // Pagination
        int totalCount = games.Count;
        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var paged = games.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new
        {
            games = paged,
            pagination = new { page, pageSize, totalCount, totalPages }
        });
    }

    // GET /api/games/:id  (public — spectator mode)
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGame(string id)
    {
        var game = await _db.GetGameByIdAsync(id);
        if (game == null) return NotFound(new { error = "Game not found" });

        var userId = GetUserId();
        bool isOwner = userId != null && userId == game.OwnerUserId;
        bool isCurrentTurn = isOwner && game.CurrentTurnPlayerId == $"player_{userId}";

        return Ok(new { game, isOwner, isCurrentTurn, spectatorMode = !isOwner });
    }

    // POST /api/games
    [HttpPost]
    public async Task<IActionResult> CreateGame([FromBody] DTOs.CreateGameRequest request)
    {
        var userId = GetUserId();
        var username = GetUsername() ?? "Commander";
        if (userId == null) return Unauthorized();

        var game = _gameEngine.CreateNewGame(
            userId, username,
            request.SelectedCountryId ?? "za",
            request.Title ?? "Global Hegemony");

        await _db.InsertGameAsync(game);
        await _analytics.UpdatePlayerStatsAsync(userId, new StatsDelta { GamesPlayed = 1 });
        return Ok(game);
    }

    // POST /api/games/:id/action
    [HttpPost("{id}/action")]
    public async Task<IActionResult> GameAction(string id, [FromBody] DTOs.GameActionRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var game = await _db.GetGameByIdAsync(id);
        if (game == null) return NotFound(new { error = "Game not found" });
        if (game.OwnerUserId != userId) return StatusCode(403, new { error = "Only the match commander can issue orders." });
        if (game.Status != "active") return BadRequest(new { error = "Game has already concluded." });

        string humanPlayerId = $"player_{userId}";
        string actionType = request.ActionType;

        // Parse payload from JsonElement
        var payload = new Dictionary<string, object?>();
        if (request.Payload.HasValue && request.Payload.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var prop in request.Payload.Value.EnumerateObject())
                payload[prop.Name] = prop.Value;
        }

        // 1. REINFORCE
        if (actionType == "reinforce")
        {
            if (game.CurrentTurnPlayerId != humanPlayerId) return BadRequest(new { error = "Not your turn." });
            if (game.PendingReinforcements <= 0) return BadRequest(new { error = "No pending reinforcements." });

            var allocs = GetAllocations(payload, "allocations");
            int total = 0;
            foreach (var (cId, count) in allocs)
            {
                if (!game.Countries.TryGetValue(cId, out var c) || c.OwnerId != humanPlayerId)
                    return BadRequest(new { error = $"You do not control territory: {cId}" });
                if (count < 0) return BadRequest(new { error = "Negative reinforcements not allowed." });
                total += count;
            }
            if (total != game.PendingReinforcements)
                return BadRequest(new { error = $"Must allocate exactly {game.PendingReinforcements} troops (allocated: {total})." });

            foreach (var (cId, count) in allocs)
                if (count > 0) game.Countries[cId].Soldiers += count;
            game.PendingReinforcements = 0;

            await _commentary.AddCommentaryAsync(game,
                $"Reinforcements deployed: Commander {game.OwnerUsername} mobilized {total} soldiers across strategic holdings.", "reinforce");
            await _db.UpdateGameAsync(game);
            return Ok(new { game });
        }

        // 2. TRANSFER
        if (actionType == "transfer")
        {
            if (game.CurrentTurnPlayerId != humanPlayerId) return BadRequest(new { error = "Not your turn." });
            if (game.ActiveBattle != null) return BadRequest(new { error = "Cannot transfer troops while an attack is in progress." });

            var fromId = GetString(payload, "fromCountryId");
            var toId = GetString(payload, "toCountryId");
            int count = GetInt(payload, "count");

            if (!game.Countries.TryGetValue(fromId, out var from) || !game.Countries.TryGetValue(toId, out var to))
                return BadRequest(new { error = "Invalid country specified." });
            if (from.OwnerId != humanPlayerId || to.OwnerId != humanPlayerId)
                return BadRequest(new { error = "Both countries must be under your command to transfer troops." });
            if (count <= 0 || from.Soldiers - count < 1)
                return BadRequest(new { error = "Territory must retain at least 1 soldier after redeployment." });

            from.Soldiers -= count;
            to.Soldiers += count;
            await _commentary.AddCommentaryAsync(game,
                $"Troop Redeployment: {count} soldiers transferred from {from.Name} to {to.Name}.", "transfer");
            await _db.UpdateGameAsync(game);
            return Ok(new { game });
        }

        // 3. START ATTACK
        if (actionType == "start_attack")
        {
            if (game.CurrentTurnPlayerId != humanPlayerId) return BadRequest(new { error = "Not your turn." });
            if (game.HasAttackedThisTurn) return BadRequest(new { error = "Only one attack permitted per turn." });
            if (game.PendingReinforcements > 0) return BadRequest(new { error = "Please allocate pending reinforcements before attacking." });

            var fromId = GetString(payload, "fromCountryId");
            var toId = GetString(payload, "toCountryId");

            if (!game.Countries.TryGetValue(fromId, out var attC) || !game.Countries.TryGetValue(toId, out var defC))
                return BadRequest(new { error = "Invalid target country." });
            if (attC.OwnerId != humanPlayerId) return BadRequest(new { error = "You do not own the attacking country." });
            if (defC.OwnerId == humanPlayerId) return BadRequest(new { error = "Cannot attack your own territory." });
            if (attC.Soldiers < 2) return BadRequest(new { error = "Attacking territory must possess at least 2 soldiers." });

            var atkPlayer = game.Players.First(p => p.Id == humanPlayerId);
            var defPlayer = game.Players.FirstOrDefault(p => p.Id == defC.OwnerId);

            game.ActiveBattle = new ActiveBattle
            {
                BattleId = $"battle_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                FromCountryId = attC.Id,
                ToCountryId = defC.Id,
                AttackerStartingSoldiers = attC.Soldiers,
                DefenderStartingSoldiers = defC.Soldiers,
                Rounds = new(),
                CurrentRoundIndex = 0,
                Status = "ongoing",
                ConquerBonus = 0
            };
            game.HasAttackedThisTurn = true;

            await _commentary.AddCommentaryAsync(game,
                _commentary.GenerateBattleStartCommentary(atkPlayer, defPlayer, attC, defC), "attack_declare");
            await _db.UpdateGameAsync(game);
            return Ok(new { game });
        }

        // 4. CLASH STEP
        if (actionType == "clash_step")
        {
            if (game.ActiveBattle == null || game.ActiveBattle.Status != "ongoing")
                return BadRequest(new { error = "No active combat skirmish in progress." });

            var b = game.ActiveBattle;
            var attC = game.Countries[b.FromCountryId];
            var defC = game.Countries[b.ToCountryId];
            var atkPlayer = game.Players.First(p => p.Id == humanPlayerId);

            var clash = _gameEngine.ExecuteClashRound(attC, defC);
            b.Rounds.Add(clash);

            var diceDelta = new Dictionary<string, int>();
            foreach (var d in clash.AttackerDice)
            {
                string k = d.ToString();
                diceDelta[k] = diceDelta.GetValueOrDefault(k, 0) + 1;
            }
            await _analytics.UpdatePlayerStatsAsync(userId, new StatsDelta
            {
                SoldiersKilled = clash.DefenderLosses,
                SoldiersLost = clash.AttackerLosses,
                DiceRolls = diceDelta
            });

            await _commentary.AddCommentaryAsync(game,
                _commentary.GenerateClashCommentary(clash.Pairings, attC, defC, clash.AttackerLosses, clash.DefenderLosses), "clash");

            var statusRes = _gameEngine.EvaluateCombatStatus(attC, defC);
            if (statusRes.Status == "conquered")
            {
                b.Status = "conquered";
                var defPlayer = game.Players.FirstOrDefault(p => p.Id == defC.OwnerId);
                var res = _gameEngine.ResolveConquest(game, atkPlayer, defPlayer, attC, defC, 0);
                b.ConquerBonus = res.ConquerBonus;
                await _analytics.UpdatePlayerStatsAsync(userId, new StatsDelta { BattlesWon = 1, CountriesConquered = 1 });
                await _commentary.AddCommentaryAsync(game,
                    _commentary.GenerateConquestCommentary(atkPlayer, defPlayer, defC, res.ConquerBonus, res.DefenderEliminated), "conquest");
                game.BattleHistory.Add(new ActiveBattle { BattleId = b.BattleId, FromCountryId = b.FromCountryId, ToCountryId = b.ToCountryId, Status = "conquered" });

                if (game.Status == "completed")
                    await _analytics.UpdatePlayerStatsAsync(userId, new StatsDelta { GamesWon = 1 });
            }
            else if (statusRes.Status == "attacker_defeated")
            {
                b.Status = "attacker_defeated";
                await _analytics.UpdatePlayerStatsAsync(userId, new StatsDelta { BattlesLost = 1 });
                await _commentary.AddCommentaryAsync(game,
                    _commentary.GenerateDefeatCommentary(atkPlayer, attC, defC), "defeat");
                game.BattleHistory.Add(new ActiveBattle { BattleId = b.BattleId, FromCountryId = b.FromCountryId, ToCountryId = b.ToCountryId, Status = "attacker_defeated" });
            }

            await _db.UpdateGameAsync(game);
            return Ok(new { game, clash });
        }

        // 5. STOP ATTACK
        if (actionType == "stop_attack")
        {
            if (game.ActiveBattle == null || game.ActiveBattle.Status != "ongoing")
                return BadRequest(new { error = "No active combat in progress." });

            var b = game.ActiveBattle;
            var attC = game.Countries[b.FromCountryId];
            var defC = game.Countries[b.ToCountryId];
            var atkPlayer = game.Players.First(p => p.Id == humanPlayerId);

            b.Status = "retreated";
            await _commentary.AddCommentaryAsync(game,
                _commentary.GenerateRetreatCommentary(atkPlayer, attC, defC), "retreat");
            game.BattleHistory.Add(new ActiveBattle { BattleId = b.BattleId, FromCountryId = b.FromCountryId, ToCountryId = b.ToCountryId, Status = "retreated" });
            game.ActiveBattle = null;

            await _db.UpdateGameAsync(game);
            return Ok(new { game });
        }

        // 6. CONQUEST TRANSFER
        if (actionType == "conquest_transfer")
        {
            if (game.ActiveBattle == null || game.ActiveBattle.Status != "conquered")
                return BadRequest(new { error = "No newly conquered territory awaiting transfer." });

            int count = GetInt(payload, "count");
            var b = game.ActiveBattle;
            var attC = game.Countries[b.FromCountryId];
            var defC = game.Countries[b.ToCountryId];

            int maxT = Math.Max(0, attC.Soldiers - 1);
            int safe = Math.Clamp(count, 0, maxT);
            if (safe > 0)
            {
                attC.Soldiers -= safe;
                defC.Soldiers += safe;
                await _commentary.AddCommentaryAsync(game,
                    $"Annexation Garrison: Transferred {safe} additional troops to secure {defC.Name}.", "conquest");
            }

            game.ActiveBattle = null;
            await _db.UpdateGameAsync(game);
            return Ok(new { game });
        }

        // 7. END TURN
        if (actionType == "end_turn" || actionType == "skip")
        {
            if (game.CurrentTurnPlayerId != humanPlayerId) return BadRequest(new { error = "Not your turn." });
            if (game.ActiveBattle?.Status == "ongoing") return BadRequest(new { error = "Please resolve or cease ongoing battle first." });

            game.ActiveBattle = null;
            _gameEngine.AdvanceTurn(game);
            await _commentary.AddCommentaryAsync(game,
                $"Commander {game.OwnerUsername} passed the initiative. Next nation begins maneuvers.", "turn");

            await _db.UpdateGameAsync(game);
            return Ok(new { game });
        }

        // 8. STEP NPC
        if (actionType == "step_npc")
        {
            await ProcessNpcTurns(game, userId, 1);
            await _db.UpdateGameAsync(game);
            return Ok(new { game });
        }

        // 9. FAST FORWARD
        if (actionType == "fast_forward")
        {
            await ProcessNpcTurns(game, userId, 15);
            await _db.UpdateGameAsync(game);
            return Ok(new { game });
        }

        return BadRequest(new { error = $"Unknown actionType: {actionType}" });
    }

    // --- Helpers ---

    private async Task ProcessNpcTurns(GameState game, string ownerId, int maxTurns)
    {
        string humanPlayerId = $"player_{ownerId}";
        int executed = 0;

        while (game.Status == "active" && game.CurrentTurnPlayerId != humanPlayerId && executed < maxTurns)
        {
            var currentPlayer = game.Players.FirstOrDefault(p => p.Id == game.CurrentTurnPlayerId);
            if (currentPlayer == null || currentPlayer.Eliminated)
            {
                _gameEngine.AdvanceTurn(game);
                executed++;
                continue;
            }

            var result = _npcEngine.SimulateNpcTurn(game, currentPlayer);

            if (result.BattleResult != null)
            {
                var b = result.BattleResult;
                if (game.Countries.TryGetValue(b.FromCountryId, out var attC) &&
                    game.Countries.TryGetValue(b.ToCountryId, out var defC))
                {
                    var defenderPlayer = game.Players.FirstOrDefault(p => p.Id == defC.OwnerId);
                    string cmtry = b.Status switch
                    {
                        "conquered" => _commentary.GenerateConquestCommentary(currentPlayer, defenderPlayer, defC, b.ConquerBonus, false),
                        "attacker_defeated" => _commentary.GenerateDefeatCommentary(currentPlayer, attC, defC),
                        _ => _commentary.GenerateRetreatCommentary(currentPlayer, attC, defC)
                    };
                    await _commentary.AddCommentaryAsync(game, cmtry, b.Status ?? "battle");
                }
            }

            _gameEngine.AdvanceTurn(game);
            executed++;
        }

        if (game.Status == "completed" && game.WinnerId?.StartsWith("player_") == true)
        {
            await _analytics.UpdatePlayerStatsAsync(ownerId, new StatsDelta { GamesWon = 1 });
            await _commentary.AddCommentaryAsync(game,
                $"🌍 SUPREME VICTORY: Commander {game.OwnerUsername} has conquered the world!", "victory");
        }
    }

    private static Dictionary<string, int> GetAllocations(Dictionary<string, object?> payload, string key)
    {
        var result = new Dictionary<string, int>();
        if (payload.TryGetValue(key, out var raw) &&
            raw is System.Text.Json.JsonElement el &&
            el.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var prop in el.EnumerateObject())
                if (prop.Value.TryGetInt32(out int val))
                    result[prop.Name] = val;
        }
        return result;
    }

    private static string GetString(Dictionary<string, object?> payload, string key)
    {
        if (payload.TryGetValue(key, out var raw))
        {
            if (raw is System.Text.Json.JsonElement el) return el.GetString() ?? string.Empty;
            return raw?.ToString() ?? string.Empty;
        }
        return string.Empty;
    }

    private static int GetInt(Dictionary<string, object?> payload, string key)
    {
        if (payload.TryGetValue(key, out var raw))
        {
            if (raw is System.Text.Json.JsonElement el && el.TryGetInt32(out int val)) return val;
            if (int.TryParse(raw?.ToString(), out int parsed)) return parsed;
        }
        return 0;
    }
}
