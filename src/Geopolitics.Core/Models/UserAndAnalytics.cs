using System.Text.Json.Serialization;

namespace Geopolitics.Core.Models;

public class UserStats
{
    [JsonPropertyName("gamesPlayed")]
    public int GamesPlayed { get; set; }

    [JsonPropertyName("gamesWon")]
    public int GamesWon { get; set; }

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

    [JsonPropertyName("diceRolls")]
    public Dictionary<string, int> DiceRolls { get; set; } = new()
    {
        { "1", 0 }, { "2", 0 }, { "3", 0 }, { "4", 0 }, { "5", 0 }, { "6", 0 }
    };
}

public class User
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [JsonPropertyName("stats")]
    public UserStats Stats { get; set; } = new();

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");

    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");
}

public class CountryCount
{
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }
}

public class CountryPick
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }
}

public class LeaderboardEntry
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("gamesWon")]
    public int GamesWon { get; set; }

    [JsonPropertyName("gamesPlayed")]
    public int GamesPlayed { get; set; }

    [JsonPropertyName("countriesConquered")]
    public int CountriesConquered { get; set; }

    [JsonPropertyName("soldiersKilled")]
    public int SoldiersKilled { get; set; }
}

public class PlayerAnalytics
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("gamesPlayed")]
    public int GamesPlayed { get; set; }

    [JsonPropertyName("gamesWon")]
    public int GamesWon { get; set; }

    [JsonPropertyName("winRate")]
    public string WinRate { get; set; } = "0%";

    [JsonPropertyName("totalBattles")]
    public int TotalBattles { get; set; }

    [JsonPropertyName("battlesWon")]
    public int BattlesWon { get; set; }

    [JsonPropertyName("battlesLost")]
    public int BattlesLost { get; set; }

    [JsonPropertyName("battleWinRate")]
    public string BattleWinRate { get; set; } = "0%";

    [JsonPropertyName("soldiersKilled")]
    public int SoldiersKilled { get; set; }

    [JsonPropertyName("soldiersLost")]
    public int SoldiersLost { get; set; }

    [JsonPropertyName("kdRatio")]
    public object KdRatio { get; set; } = 0;

    [JsonPropertyName("countriesConquered")]
    public int CountriesConquered { get; set; }

    [JsonPropertyName("topStartingCountries")]
    public List<CountryCount> TopStartingCountries { get; set; } = new();

    [JsonPropertyName("diceRolls")]
    public Dictionary<string, int> DiceRolls { get; set; } = new();
}

public class PlatformAnalytics
{
    [JsonPropertyName("totalUsers")]
    public int TotalUsers { get; set; }

    [JsonPropertyName("totalGames")]
    public int TotalGames { get; set; }

    [JsonPropertyName("activeGames")]
    public int ActiveGames { get; set; }

    [JsonPropertyName("completedGames")]
    public int CompletedGames { get; set; }

    [JsonPropertyName("avgRoundsPerGame")]
    public string AvgRoundsPerGame { get; set; } = "0";

    [JsonPropertyName("globalDiceRolls")]
    public Dictionary<string, int> GlobalDiceRolls { get; set; } = new();

    [JsonPropertyName("popularStartingCountries")]
    public List<CountryPick> PopularStartingCountries { get; set; } = new();

    [JsonPropertyName("leaderboard")]
    public List<LeaderboardEntry> Leaderboard { get; set; } = new();
}

