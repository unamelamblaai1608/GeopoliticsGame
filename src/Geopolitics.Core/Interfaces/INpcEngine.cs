using Geopolitics.Core.Engines;
using Geopolitics.Core.Models;

namespace Geopolitics.Core.Interfaces;

/// <summary>
/// NPC engine — simulates AI player turns with personality-driven decisions.
/// Supports aggressive, defensive, calculating, and opportunist archetypes.
/// </summary>
public interface INpcEngine
{
    NpcTurnSimulationResult SimulateNpcTurn(GameState game, Player npcPlayer);
}
