using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Geopolitics.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace Geopolitics.Core.Services;

public class AuthResult
{
    public User User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}

public class AuthService
{
    private readonly IDatabaseService _db;
    private readonly string _jwtSecret;

    public AuthService(IDatabaseService db, string? jwtSecret = null)
    {
        _db = db;
        _jwtSecret = jwtSecret ?? Environment.GetEnvironmentVariable("JWT_SECRET") ?? "geopolitics-super-secret-key-2026-csharp-version";
    }

    public async Task<AuthResult> RegisterAsync(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("All fields are required.");
        }

        var existingUser = await _db.GetUserByUsernameOrEmailAsync(username);
        if (existingUser != null && existingUser.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Username is already taken");
        }

        var existingEmail = await _db.GetUserByUsernameOrEmailAsync(email);
        if (existingEmail != null && existingEmail.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Email is already registered");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var newUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = username.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Stats = new UserStats()
        };

        await _db.InsertUserAsync(newUser);
        var token = GenerateToken(newUser.Id, newUser.Username);

        return new AuthResult
        {
            User = newUser,
            Token = token
        };
    }

    public async Task<AuthResult> LoginAsync(string identifier, string password)
    {
        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Username/Email and password required.");
        }

        var user = await _db.GetUserByUsernameOrEmailAsync(identifier);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        bool match = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!match)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var token = GenerateToken(user.Id, user.Username);

        return new AuthResult
        {
            User = user,
            Token = token
        };
    }

    public string GenerateToken(string userId, string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecret.PadRight(32, 'x')); // Ensure at least 256 bits

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("userId", userId),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username),
                new Claim("username", username)
            }),
            Expires = DateTime.UtcNow.AddDays(30),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecret.PadRight(32, 'x'));

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.FromMinutes(5)
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}

