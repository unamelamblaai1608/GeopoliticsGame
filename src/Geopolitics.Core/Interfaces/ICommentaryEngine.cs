using Geopolitics.Core.Models;

namespace Geopolitics.Core.Interfaces;

/// <summary>
/// Commentary engine — generates dramatic battle narrative text,
/// optionally enhanced with Gemini AI for dynamic commentary.
/// </summary>
public interface ICommentaryEngine
{
    Task<CommentaryEntry> AddCommentaryAsync(GameState game, string text, string type = "battle", string? apiKey = null);
    string GenerateBattleStartCommentary(Player attacker, Player? defender, Country attackerCountry, Country defenderCountry);
    string GenerateClashCommentary(List<DicePairing> pairings, Country attackerCountry, Country defenderCountry, int attackerLosses, int defenderLosses);
    string GenerateConquestCommentary(Player attacker, Player? defender, Country conquered, int bonus, bool defenderEliminated);
    string GenerateRetreatCommentary(Player attacker, Country attackerCountry, Country defenderCountry);
    string GenerateDefeatCommentary(Player attacker, Country attackerCountry, Country defenderCountry);
}

