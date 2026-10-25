using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Geopolitics.Core.Interfaces;

namespace Geopolitics.Web.Controllers;

/// <summary>
/// Analytics API controller — player-specific stats and platform-wide metrics.
/// </summary>
[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsEngine _analytics;

    public AnalyticsController(IAnalyticsEngine analytics)
    {
        _analytics = analytics;
    }

    // GET /api/analytics/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> PlayerAnalytics()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var data = await _analytics.GetPlayerAnalyticsAsync(userId);
        return Ok(data);
    }

    // GET /api/analytics/platform
    [HttpGet("platform")]
    [AllowAnonymous]
    public async Task<IActionResult> PlatformAnalytics()
    {
        var data = await _analytics.GetPlatformAnalyticsAsync();
        return Ok(data);
    }
}
