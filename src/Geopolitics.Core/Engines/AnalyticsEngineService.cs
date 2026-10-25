using Geopolitics.Core.Interfaces;
using Geopolitics.Core.Models;
using Geopolitics.Core.Services;

namespace Geopolitics.Core.Engines;

/// <summary>
/// Instance-based analytics engine implementing IAnalyticsEngine.
/// Injects IDatabaseService for data access — demonstrates proper
/// encapsulation and dependency injection over static method calls.
/// </summary>
public class AnalyticsEngineService : IAnalyticsEngine
{
    private readonly IDatabaseService _db;

    public AnalyticsEngineService(IDatabaseService db)
    {
        _db = db;
    }

    public Task UpdatePlayerStatsAsync(string userId, StatsDelta delta)
        => AnalyticsEngine.UpdatePlayerStatsAsync(_db, userId, delta);

    public Task<PlayerAnalytics> GetPlayerAnalyticsAsync(string userId)
        => AnalyticsEngine.GetPlayerAnalyticsAsync(_db, userId);

    public Task<PlatformAnalytics> GetPlatformAnalyticsAsync()
        => AnalyticsEngine.GetPlatformAnalyticsAsync(_db);
}

