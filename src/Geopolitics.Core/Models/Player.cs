using System.Text.Json.Serialization;

namespace Geopolitics.Core.Models;

public class PlayerBattleStats
{
    [JsonPropertyName("battlesWon")]
    public int BattlesWon { get; set; }

    [JsonPropertyName("battlesLost")]
    public int BattlesLost { get; set; }

    [JsonPropertyName("soldiersKilled")]
    public int SoldiersKilled { get; set; }

    [JsonPropertyName("soldiersLost")]
    public int SoldiersLost { get; set; }

    [JsonPropertyName("countriesConquered")]
    public int CountriesConquered { get; set; }
}

public class Player
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("isHuman")]
    public bool IsHuman { get; set; }

    [JsonPropertyName("startingCountryId")]
    public string StartingCountryId { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#3b82f6";

    [JsonPropertyName("personality")]
    public string Personality { get; set; } = "human";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "Commander";

    [JsonPropertyName("eliminated")]
    public bool Eliminated { get; set; }

    [JsonPropertyName("stats")]
    public PlayerBattleStats Stats { get; set; } = new();
}

