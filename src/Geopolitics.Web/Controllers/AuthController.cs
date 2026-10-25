using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Geopolitics.Web.DTOs;
using Geopolitics.Core.Services;
using Geopolitics.Core.Models;

namespace Geopolitics.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IDatabaseService _dbService;

    public AuthController(AuthService authService, IDatabaseService dbService)
    {
        _authService = authService;
        _dbService = dbService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request.Username, request.Email, request.Password);
            var userDto = new { id = result.User.Id, username = result.User.Username, email = result.User.Email, stats = result.User.Stats };
            return Ok(new { message = "User registered", token = result.Token, user = userDto });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request.Identifier, request.Password);
            var userDto = new { id = result.User.Id, username = result.User.Username, email = result.User.Email, stats = result.User.Stats };
            return Ok(new { token = result.Token, user = userDto });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var user = await _dbService.GetUserByIdAsync(userId);
        if (user == null) return NotFound(new { error = "User not found" });

        return Ok(new { id = user.Id, username = user.Username, email = user.Email, stats = user.Stats });
    }
}
