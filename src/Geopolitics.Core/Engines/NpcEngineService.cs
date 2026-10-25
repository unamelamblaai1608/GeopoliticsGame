using Geopolitics.Core.Interfaces;
using Geopolitics.Core.Models;

namespace Geopolitics.Core.Engines;

/// <summary>
/// Instance-based NPC engine implementing INpcEngine.
/// Wraps static NpcEngine for DI compatibility and testability.
/// </summary>
public class NpcEngineService : INpcEngine
{
    public NpcTurnSimulationResult SimulateNpcTurn(GameState game, Player npcPlayer)
        => NpcEngine.SimulateNpcTurn(game, npcPlayer);
}

