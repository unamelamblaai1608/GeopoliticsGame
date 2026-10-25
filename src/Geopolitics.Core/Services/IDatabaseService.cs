using Geopolitics.Core.Models;

namespace Geopolitics.Core.Services;

public interface IDatabaseService
{
    Task InitAsync();
    
    // User operations
    Task<User?> GetUserByIdAsync(string id);
    Task<User?> GetUserByUsernameOrEmailAsync(string identifier);
    Task<User> InsertUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<List<User>> GetAllUsersAsync();

    // Game operations
    Task<GameState?> GetGameByIdAsync(string id);
    Task<List<GameState>> GetGamesByOwnerAsync(string ownerUserId);
    Task<GameState> InsertGameAsync(GameState game);
    Task<bool> UpdateGameAsync(GameState game);
    Task<List<GameState>> GetAllGamesAsync();
}

