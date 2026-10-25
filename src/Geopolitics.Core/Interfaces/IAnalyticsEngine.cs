using Geopolitics.Core.Engines;
using Geopolitics.Core.Models;

namespace Geopolitics.Core.Interfaces;

/// <summary>
/// Analytics engine — computes player statistics, platform-wide metrics,
/// leaderboards, and dice roll histograms.
/// </summary>
public interface IAnalyticsEngine
{
    Task UpdatePlayerStatsAsync(string userId, StatsDelta delta);
    Task<PlayerAnalytics> GetPlayerAnalyticsAsync(string userId);
    Task<PlatformAnalytics> GetPlatformAnalyticsAsync();
}

