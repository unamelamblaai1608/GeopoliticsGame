using System.Text.Json;
using Geopolitics.Core.Models;

namespace Geopolitics.Core.Services;

public class JsonDatabaseService : IDatabaseService
{
    private readonly string _dataDir;
    private readonly string _usersPath;
    private readonly string _gamesPath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private List<User> _users = new();
    private List<GameState> _games = new();
    private bool _initialized = false;

    public JsonDatabaseService(string? dataDir = null)
    {
        _dataDir = dataDir ?? Path.Combine(AppContext.BaseDirectory, "data");
        _usersPath = Path.Combine(_dataDir, "users.json");
        _gamesPath = Path.Combine(_dataDir, "games.json");
    }

    public async Task InitAsync()
    {
        if (_initialized) return;

        await _lock.WaitAsync();
        try
        {
            if (!Directory.Exists(_dataDir))
            {
                Directory.CreateDirectory(_dataDir);
            }

            if (File.Exists(_usersPath))
            {
                var json = await File.ReadAllTextAsync(_usersPath);
                _users = JsonSerializer.Deserialize<List<User>>(json, _jsonOptions) ?? new();
            }
            else
            {
                _users = new();
            }

            if (File.Exists(_gamesPath))
            {
                var json = await File.ReadAllTextAsync(_gamesPath);
                _games = JsonSerializer.Deserialize<List<GameState>>(json, _jsonOptions) ?? new();
            }
            else
            {
                _games = new();
            }

            // Seed default test user if missing: commander / password123
            if (!_users.Any(u => u.Username.Equals("commander", StringComparison.OrdinalIgnoreCase)))
            {
                var defaultUser = new User
                {
                    Id = "seed_user_commander",
                    Username = "commander",
                    Email = "commander@risk.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Stats = new UserStats
                    {
                        GamesPlayed = 3,
                        GamesWon = 1,
                        BattlesWon = 8,
                        BattlesLost = 3,
                        SoldiersKilled = 22,
                        SoldiersLost = 14,
                        CountriesConquered = 4,
                        DiceRolls = new()
                        {
                            { "1", 4 }, { "2", 5 }, { "3", 6 }, { "4", 8 }, { "5", 9 }, { "6", 7 }
                        }
                    }
                };
                _users.Add(defaultUser);
                await SaveUsersInternalAsync();
            }

            _initialized = true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task SaveUsersInternalAsync()
    {
        var temp = $"{_usersPath}.tmp";
        var json = JsonSerializer.Serialize(_users, _jsonOptions);
        await File.WriteAllTextAsync(temp, json);
        File.Move(temp, _usersPath, true);
    }

    private async Task SaveGamesInternalAsync()
    {
        var temp = $"{_gamesPath}.tmp";
        var json = JsonSerializer.Serialize(_games, _jsonOptions);
        await File.WriteAllTextAsync(temp, json);
        File.Move(temp, _gamesPath, true);
    }

    public async Task<User?> GetUserByIdAsync(string id)
    {
        await InitAsync();
        await _lock.WaitAsync();
        try
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<User?> GetUserByUsernameOrEmailAsync(string identifier)
    {
        await InitAsync();
        await _lock.WaitAsync();
        try
        {
            var clean = identifier.Trim().ToLowerInvariant();
            return _users.FirstOrDefault(u => u.Username.ToLowerInvariant() == clean || u.Email.ToLowerInvariant() == clean);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<User> InsertUserAsync(User user)
    {
        await InitAsync();
        await _lock.WaitAsync();
        try
        {
            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = Guid.NewGuid().ToString();
            }
            user.CreatedAt = DateTime.UtcNow.ToString("o");
            user.UpdatedAt = DateTime.UtcNow.ToString("o");
            _users.Add(user);
            await SaveUsersInternalAsync();
            return user;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        await InitAsync();
        await _lock.WaitAsync();
        try
        {
            var idx = _users.FindIndex(u => u.Id == user.Id);
            if (idx == -1) return false;

            user.UpdatedAt = DateTime.UtcNow.ToString("o");
            _users[idx] = user;
            await SaveUsersInternalAsync();
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        await InitAsync();
        await _lock.WaitAsync();
        try
        {
            return _users.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<GameState?> GetGameByIdAsync(string id)
    {
        await InitAsync();
        await _lock.WaitAsync();
        try
        {
            return _games.FirstOrDefault(g => g.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<GameState>> GetGamesByOwnerAsync(string ownerUserId)
    {
        await InitAsync();
        await _lock.WaitAsync();
        try
        {
            return _games.Where(g => g.OwnerUserId == ownerUserId).OrderByDescending(g => g.CreatedAt).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<GameState> InsertGameAsync(GameState game)
    {
        await InitAsync();
        await _lock.WaitAsync();
        try
        {
            if (string.IsNullOrEmpty(game.Id))
            {
                game.Id = Guid.NewGuid().ToString();
            }
            _games.Add(game);
            await SaveGamesInternalAsync();
            return game;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> UpdateGameAsync(GameState game)
    {
        await InitAsync();
        await _lock.WaitAsync();
        try
        {
            var idx = _games.FindIndex(g => g.Id == game.Id);
            if (idx == -1) return false;

            _games[idx] = game;
            await SaveGamesInternalAsync();
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<GameState>> GetAllGamesAsync()
    {
        await InitAsync();
        await _lock.WaitAsync();
        try
        {
            return _games.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }
}

