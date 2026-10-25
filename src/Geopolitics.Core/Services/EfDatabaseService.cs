using System.Text.Json;
using Geopolitics.Core.Data;
using Geopolitics.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Geopolitics.Core.Services;

/// <summary>
/// Entity Framework Core implementation of IDatabaseService using SQLite.
/// Stores complex nested game state as JSON columns for flexibility while
/// maintaining relational integrity for users and indexed lookups.
/// </summary>
public class EfDatabaseService : IDatabaseService
{
    private readonly IDbContextFactory<GeopoliticsDbContext> _contextFactory;
    private bool _initialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public EfDatabaseService(IDbContextFactory<GeopoliticsDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task InitAsync()
    {
        if (_initialized) return;
        await _initLock.WaitAsync();
        try
        {
            if (_initialized) return;
            await using var db = await _contextFactory.CreateDbContextAsync();
            await db.Database.EnsureCreatedAsync();

            // Seed default test user if missing: commander / password123
            if (!await db.Users.AnyAsync(u => u.Username == "commander"))
            {
                db.Users.Add(new UserEntity
                {
                    Id = "seed_user_commander",
                    Username = "commander",
                    Email = "commander@risk.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    StatsJson = JsonSerializer.Serialize(new UserStats
                    {
                        GamesPlayed = 3, GamesWon = 1, BattlesWon = 8, BattlesLost = 3,
                        SoldiersKilled = 22, SoldiersLost = 14, CountriesConquered = 4,
                        DiceRolls = new() { {"1",4},{"2",5},{"3",6},{"4",8},{"5",9},{"6",7} }
                    }, JsonOpts),
                    CreatedAt = DateTime.UtcNow.ToString("o"),
                    UpdatedAt = DateTime.UtcNow.ToString("o")
                });
                await db.SaveChangesAsync();
            }
            _initialized = true;
        }
        finally { _initLock.Release(); }
    }

    // ---- User operations ----

    public async Task<User?> GetUserByIdAsync(string id)
    {
        await InitAsync();
        await using var db = await _contextFactory.CreateDbContextAsync();
        var e = await db.Users.FindAsync(id);
        return e == null ? null : ToUser(e);
    }

    public async Task<User?> GetUserByUsernameOrEmailAsync(string identifier)
    {
        await InitAsync();
        var clean = identifier.Trim().ToLowerInvariant();
        await using var db = await _contextFactory.CreateDbContextAsync();
        var e = await db.Users.FirstOrDefaultAsync(u =>
            u.Username.ToLower() == clean || u.Email.ToLower() == clean);
        return e == null ? null : ToUser(e);
    }

    public async Task<User> InsertUserAsync(User user)
    {
        await InitAsync();
        if (string.IsNullOrEmpty(user.Id)) user.Id = Guid.NewGuid().ToString();
        user.CreatedAt = DateTime.UtcNow.ToString("o");
        user.UpdatedAt = DateTime.UtcNow.ToString("o");
        await using var db = await _contextFactory.CreateDbContextAsync();
        db.Users.Add(ToEntity(user));
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        await InitAsync();
        await using var db = await _contextFactory.CreateDbContextAsync();
        var e = await db.Users.FindAsync(user.Id);
        if (e == null) return false;
        e.Username = user.Username;
        e.Email = user.Email;
        e.PasswordHash = user.PasswordHash;
        e.StatsJson = JsonSerializer.Serialize(user.Stats ?? new UserStats(), JsonOpts);
        e.UpdatedAt = DateTime.UtcNow.ToString("o");
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        await InitAsync();
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entities = await db.Users.ToListAsync();
        return entities.Select(ToUser).ToList();
    }

    // ---- Game operations ----

    public async Task<GameState?> GetGameByIdAsync(string id)
    {
        await InitAsync();
        await using var db = await _contextFactory.CreateDbContextAsync();
        var e = await db.Games.FindAsync(id);
        return e == null ? null : ToGameState(e);
    }

    public async Task<List<GameState>> GetGamesByOwnerAsync(string ownerUserId)
    {
        await InitAsync();
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entities = await db.Games
            .Where(g => g.OwnerUserId == ownerUserId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
        return entities.Select(ToGameState).ToList();
    }

    public async Task<GameState> InsertGameAsync(GameState game)
    {
        await InitAsync();
        if (string.IsNullOrEmpty(game.Id)) game.Id = Guid.NewGuid().ToString();
        await using var db = await _contextFactory.CreateDbContextAsync();
        db.Games.Add(ToGameEntity(game));
        await db.SaveChangesAsync();
        return game;
    }

    public async Task<bool> UpdateGameAsync(GameState game)
    {
        await InitAsync();
        await using var db = await _contextFactory.CreateDbContextAsync();
        var e = await db.Games.FindAsync(game.Id);
        if (e == null) return false;
        e.Status = game.Status;
        e.Title = game.Title;
        e.GameJson = JsonSerializer.Serialize(game, JsonOpts);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<GameState>> GetAllGamesAsync()
    {
        await InitAsync();
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entities = await db.Games.ToListAsync();
        return entities.Select(ToGameState).ToList();
    }

    // ---- Mapping helpers ----

    private static User ToUser(UserEntity e) => new()
    {
        Id = e.Id,
        Username = e.Username,
        Email = e.Email,
        PasswordHash = e.PasswordHash,
        Stats = JsonSerializer.Deserialize<UserStats>(e.StatsJson, JsonOpts) ?? new UserStats(),
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static UserEntity ToEntity(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email,
        PasswordHash = u.PasswordHash,
        StatsJson = JsonSerializer.Serialize(u.Stats ?? new UserStats(), JsonOpts),
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt
    };

    private static GameState ToGameState(GameEntity e)
    {
        var gs = JsonSerializer.Deserialize<GameState>(e.GameJson, JsonOpts);
        return gs ?? new GameState { Id = e.Id, OwnerUserId = e.OwnerUserId, Status = e.Status };
    }

    private static GameEntity ToGameEntity(GameState g) => new()
    {
        Id = g.Id,
        OwnerUserId = g.OwnerUserId,
        OwnerUsername = g.OwnerUsername,
        Title = g.Title,
        Status = g.Status,
        CreatedAt = g.CreatedAt,
        GameJson = JsonSerializer.Serialize(g, JsonOpts)
    };
}

