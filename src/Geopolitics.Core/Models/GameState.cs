using System.Text.Json.Serialization;

namespace Geopolitics.Core.Models;

public class GameState
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("title")]
    public string Title { get; set; } = "Global Conquest";

    [JsonPropertyName("ownerUserId")]
    public string OwnerUserId { get; set; } = string.Empty;

    [JsonPropertyName("ownerUsername")]
    public string OwnerUsername { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");

    [JsonPropertyName("status")]
    public string Status { get; set; } = "active"; // active, completed

    [JsonPropertyName("winnerId")]
    public string? WinnerId { get; set; }

    [JsonPropertyName("winnerName")]
    public string? WinnerName { get; set; }

    [JsonPropertyName("roundNumber")]
    public int RoundNumber { get; set; } = 1;

    [JsonPropertyName("turnNumber")]
    public int TurnNumber { get; set; } = 1;

    [JsonPropertyName("turnOrder")]
    public List<string> TurnOrder { get; set; } = new();

    [JsonPropertyName("currentTurnIndex")]
    public int CurrentTurnIndex { get; set; }

    [JsonPropertyName("currentTurnPlayerId")]
    public string CurrentTurnPlayerId { get; set; } = string.Empty;

    [JsonPropertyName("players")]
    public List<Player> Players { get; set; } = new();

    [JsonPropertyName("countries")]
    public Dictionary<string, Country> Countries { get; set; } = new();

    [JsonPropertyName("pendingReinforcements")]
    public int PendingReinforcements { get; set; }

    [JsonPropertyName("hasAttackedThisTurn")]
    public bool HasAttackedThisTurn { get; set; }

    [JsonPropertyName("activeBattle")]
    public ActiveBattle? ActiveBattle { get; set; }

    [JsonPropertyName("battleHistory")]
    public List<ActiveBattle> BattleHistory { get; set; } = new();

    [JsonPropertyName("npcDecisionLog")]
    public List<NpcDecisionLogEntry> NpcDecisionLog { get; set; } = new();

    [JsonPropertyName("recentCommentary")]
    public List<CommentaryEntry> RecentCommentary { get; set; } = new();
}

