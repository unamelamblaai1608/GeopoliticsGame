using Geopolitics.Core.Engines;
using Geopolitics.Core.Models;

namespace Geopolitics.Core.Interfaces;

/// <summary>
/// Core game engine — creates games, advances turns, executes combat.
/// Encapsulates all turn-based strategy logic behind a testable interface.
/// </summary>
public interface IGameEngine
{
    GameState CreateNewGame(string userId, string username, string? selectedCountryId = "za", string? title = "Global Conquest");
    GameState AdvanceTurn(GameState game);
    ClashResult ExecuteClashRound(Country attackerCountry, Country defenderCountry);
    CombatStatusResult EvaluateCombatStatus(Country attackerCountry, Country defenderCountry);
    ConquestResolution ResolveConquest(GameState game, Player attackerPlayer, Player? defenderPlayer, Country attackerCountry, Country defenderCountry, int transferCount = 0);
    int CalculatePlayerReinforcements(GameState game, string playerId);
}
