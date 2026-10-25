using Geopolitics.Core.Interfaces;
using Geopolitics.Core.Models;

namespace Geopolitics.Core.Engines;

/// <summary>
/// Instance-based game engine service implementing IGameEngine.
/// Delegates to the static GameEngine for all logic while enabling
/// proper dependency injection, mocking, and unit testing.
/// </summary>
public class GameEngineService : IGameEngine
{
    public GameState CreateNewGame(string userId, string username, string? selectedCountryId = "za", string? title = "Global Conquest")
        => GameEngine.CreateNewGame(userId, username, selectedCountryId, title);

    public GameState AdvanceTurn(GameState game)
        => GameEngine.AdvanceTurn(game);

    public ClashResult ExecuteClashRound(Country attackerCountry, Country defenderCountry)
        => GameEngine.ExecuteClashRound(attackerCountry, defenderCountry);

    public CombatStatusResult EvaluateCombatStatus(Country attackerCountry, Country defenderCountry)
        => GameEngine.EvaluateCombatStatus(attackerCountry, defenderCountry);

    public ConquestResolution ResolveConquest(GameState game, Player attackerPlayer, Player? defenderPlayer, Country attackerCountry, Country defenderCountry, int transferCount = 0)
        => GameEngine.ResolveConquest(game, attackerPlayer, defenderPlayer, attackerCountry, defenderCountry, transferCount);

    public int CalculatePlayerReinforcements(GameState game, string playerId)
        => GameEngine.CalculatePlayerReinforcements(game, playerId);
}

