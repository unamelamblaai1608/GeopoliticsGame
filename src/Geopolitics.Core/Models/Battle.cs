using System.Text.Json.Serialization;

namespace Geopolitics.Core.Models;

public class DicePairing
{
    [JsonPropertyName("attackerDie")]
    public int AttackerDie { get; set; }

    [JsonPropertyName("defenderDie")]
    public int DefenderDie { get; set; }

    [JsonPropertyName("outcome")]
    public string Outcome { get; set; } = "tie"; // attacker_won, defender_won, tie
}

public class ClashResult
{
    [JsonPropertyName("diceCount")]
    public int DiceCount { get; set; }

    [JsonPropertyName("attackerDice")]
    public List<int> AttackerDice { get; set; } = new();

    [JsonPropertyName("defenderDice")]
    public List<int> DefenderDice { get; set; } = new();

    [JsonPropertyName("pairings")]
    public List<DicePairing> Pairings { get; set; } = new();

    [JsonPropertyName("attackerLosses")]
    public int AttackerLosses { get; set; }

    [JsonPropertyName("defenderLosses")]
    public int DefenderLosses { get; set; }

    [JsonPropertyName("attackerRemaining")]
    public int AttackerRemaining { get; set; }

    [JsonPropertyName("defenderRemaining")]
    public int DefenderRemaining { get; set; }
}

public class ActiveBattle
{
    [JsonPropertyName("battleId")]
    public string BattleId { get; set; } = string.Empty;

    [JsonPropertyName("fromCountryId")]
    public string FromCountryId { get; set; } = string.Empty;

    [JsonPropertyName("toCountryId")]
    public string ToCountryId { get; set; } = string.Empty;

    [JsonPropertyName("attackerStartingSoldiers")]
    public int AttackerStartingSoldiers { get; set; }

    [JsonPropertyName("defenderStartingSoldiers")]
    public int DefenderStartingSoldiers { get; set; }

    [JsonPropertyName("rounds")]
    public List<ClashResult> Rounds { get; set; } = new();

    [JsonPropertyName("currentRoundIndex")]
    public int CurrentRoundIndex { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "ongoing"; // ongoing, conquered, attacker_defeated, retreated

    [JsonPropertyName("conquerBonus")]
    public int ConquerBonus { get; set; }

    [JsonPropertyName("outcome")]
    public string? Outcome { get; set; }

    [JsonPropertyName("roundNumber")]
    public int? RoundNumber { get; set; }

    [JsonPropertyName("turnNumber")]
    public int? TurnNumber { get; set; }

    [JsonPropertyName("attackerPlayerId")]
    public string? AttackerPlayerId { get; set; }

    [JsonPropertyName("attackerPlayerName")]
    public string? AttackerPlayerName { get; set; }

    [JsonPropertyName("defenderPlayerId")]
    public string? DefenderPlayerId { get; set; }

    [JsonPropertyName("defenderPlayerName")]
    public string? DefenderPlayerName { get; set; }

    [JsonPropertyName("attackerCountryId")]
    public string? AttackerCountryId { get; set; }

    [JsonPropertyName("attackerCountryName")]
    public string? AttackerCountryName { get; set; }

    [JsonPropertyName("defenderCountryId")]
    public string? DefenderCountryId { get; set; }

    [JsonPropertyName("defenderCountryName")]
    public string? DefenderCountryName { get; set; }

    [JsonPropertyName("conquered")]
    public bool? Conquered { get; set; }

    [JsonPropertyName("defenderEliminated")]
    public bool? DefenderEliminated { get; set; }
}

