using Geopolitics.Core.Models;
using Geopolitics.Core.Services;

namespace Geopolitics.Core.Engines;

public class StatsDelta
{
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int BattlesWon { get; set; }
    public int BattlesLost { get; set; }
    public int SoldiersKilled { get; set; }
    public int SoldiersLost { get; set; }
    public int CountriesConquered { get; set; }
    public Dictionary<string, int>? DiceRolls { get; set; }
}

public static class AnalyticsEngine
{
    public static async Task UpdatePlayerStatsAsync(IDatabaseService db, string userId, StatsDelta delta)
    {
        if (string.IsNullOrEmpty(userId)) return;

        var user = await db.GetUserByIdAsync(userId);
        if (user == null) return;

        var stats = user.Stats ??= new UserStats();

        stats.GamesPlayed += delta.GamesPlayed;
        stats.GamesWon += delta.GamesWon;
        stats.BattlesWon += delta.BattlesWon;
        stats.BattlesLost += delta.BattlesLost;
        stats.SoldiersKilled += delta.SoldiersKilled;
        stats.SoldiersLost += delta.SoldiersLost;
        stats.CountriesConquered += delta.CountriesConquered;

        if (delta.DiceRolls != null)
        {
            foreach (var (die, count) in delta.DiceRolls)
            {
                if (!stats.DiceRolls.ContainsKey(die))
                {
                    stats.DiceRolls[die] = 0;
                }
                stats.DiceRolls[die] += count;
            }
        }

        await db.UpdateUserAsync(user);
    }

    public static async Task<PlayerAnalytics> GetPlayerAnalyticsAsync(IDatabaseService db, string userId)
    {
        var user = await db.GetUserByIdAsync(userId);
        if (user == null) throw new InvalidOperationException("User not found");

        var userGames = await db.GetGamesByOwnerAsync(userId);
        var stats = user.Stats ?? new UserStats();

        int gamesPlayed = stats.GamesPlayed > 0 ? stats.GamesPlayed : userGames.Count;
        int gamesWon = stats.GamesWon > 0
            ? stats.GamesWon
            : userGames.Count(g => g.WinnerId != null && g.WinnerId.StartsWith($"player_{userId}"));

        double winRateVal = gamesPlayed > 0 ? (gamesWon / (double)gamesPlayed) * 100 : 0;
        string winRate = $"{winRateVal:F1}%";

        int battlesWon = stats.BattlesWon;
        int battlesLost = stats.BattlesLost;
        int totalBattles = battlesWon + battlesLost;
        double battleWinRateVal = totalBattles > 0 ? (battlesWon / (double)totalBattles) * 100 : 0;
        string battleWinRate = $"{battleWinRateVal:F1}%";

        int soldiersKilled = stats.SoldiersKilled;
        int soldiersLost = stats.SoldiersLost;
        string kdRatio = soldiersLost > 0 ? $"{(soldiersKilled / (double)soldiersLost):F2}" : soldiersKilled.ToString();

        var countryCounts = new Dictionary<string, int>();
        foreach (var g in userGames)
        {
            var human = g.Players.FirstOrDefault(p => p.IsHuman);
            if (human != null && !string.IsNullOrEmpty(human.StartingCountryId))
            {
                string name = g.Countries.TryGetValue(human.StartingCountryId, out var c) ? c.Name : human.StartingCountryId;
                countryCounts[name] = countryCounts.GetValueOrDefault(name, 0) + 1;
            }
        }

        var topCountries = countryCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .Select(kvp => new CountryCount { Country = kvp.Key, Count = kvp.Value })
            .ToList();

        return new PlayerAnalytics
        {
            Username = user.Username,
            GamesPlayed = gamesPlayed,
            GamesWon = gamesWon,
            WinRate = winRate,
            TotalBattles = totalBattles,
            BattlesWon = battlesWon,
            BattlesLost = battlesLost,
            BattleWinRate = battleWinRate,
            SoldiersKilled = soldiersKilled,
            SoldiersLost = soldiersLost,
            KdRatio = kdRatio,
            CountriesConquered = stats.CountriesConquered,
            TopStartingCountries = topCountries,
            DiceRolls = stats.DiceRolls
        };
    }

    public static async Task<PlatformAnalytics> GetPlatformAnalyticsAsync(IDatabaseService db)
    {
        var allGames = await db.GetAllGamesAsync();
        var allUsers = await db.GetAllUsersAsync();

        int totalGames = allGames.Count;
        var completedGames = allGames.Where(g => g.Status == "completed").ToList();
        var activeGames = allGames.Where(g => g.Status == "active").ToList();

        int totalRounds = completedGames.Sum(g => g.RoundNumber);
        string avgRounds = completedGames.Count > 0
            ? $"{(totalRounds / (double)completedGames.Count):F1}"
            : "0";

        var globalDice = new Dictionary<string, int>
        {
            { "1", 0 }, { "2", 0 }, { "3", 0 }, { "4", 0 }, { "5", 0 }, { "6", 0 }
        };

        foreach (var u in allUsers)
        {
            if (u.Stats?.DiceRolls != null)
            {
                for (int i = 1; i <= 6; i++)
                {
                    string key = i.ToString();
                    globalDice[key] += u.Stats.DiceRolls.GetValueOrDefault(key, 0);
                }
            }
        }

        var countryPicks = new Dictionary<string, int>();
        foreach (var g in allGames)
        {
            var human = g.Players.FirstOrDefault(p => p.IsHuman);
            if (human != null && !string.IsNullOrEmpty(human.StartingCountryId))
            {
                string name = g.Countries.TryGetValue(human.StartingCountryId, out var c) ? c.Name : human.StartingCountryId;
                countryPicks[name] = countryPicks.GetValueOrDefault(name, 0) + 1;
            }
        }

        var popularCountries = countryPicks
            .OrderByDescending(kvp => kvp.Value)
            .Take(10)
            .Select(kvp => new CountryPick { Name = kvp.Key, Count = kvp.Value })
            .ToList();

        var leaderboard = allUsers
            .Select(u => new LeaderboardEntry
            {
                Username = u.Username,
                GamesWon = u.Stats?.GamesWon ?? 0,
                GamesPlayed = u.Stats?.GamesPlayed ?? 0,
                CountriesConquered = u.Stats?.CountriesConquered ?? 0,
                SoldiersKilled = u.Stats?.SoldiersKilled ?? 0
            })
            .OrderByDescending(u => u.GamesWon)
            .ThenByDescending(u => u.CountriesConquered)
            .Take(10)
            .ToList();

        return new PlatformAnalytics
        {
            TotalUsers = allUsers.Count,
            TotalGames = totalGames,
            ActiveGames = activeGames.Count,
            CompletedGames = completedGames.Count,
            AvgRoundsPerGame = avgRounds,
            GlobalDiceRolls = globalDice,
            PopularStartingCountries = popularCountries,
            Leaderboard = leaderboard
        };
    }
}

