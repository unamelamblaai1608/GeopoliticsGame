using Geopolitics.Core.Interfaces;
using Geopolitics.Core.Models;

namespace Geopolitics.Core.Engines;

/// <summary>
/// Instance-based commentary engine implementing ICommentaryEngine.
/// Provides dramatic battle narrative generation with optional Gemini AI enhancement.
/// The API key is injected via constructor for secure, testable configuration.
/// </summary>
public class CommentaryEngineService : ICommentaryEngine
{
    private readonly string _geminiApiKey;

    public CommentaryEngineService(string geminiApiKey = "")
    {
        _geminiApiKey = geminiApiKey;
    }

    public Task<CommentaryEntry> AddCommentaryAsync(GameState game, string text, string type = "battle", string? apiKey = null)
        => CommentaryEngine.AddCommentaryToGameAsync(game, text, type, apiKey ?? _geminiApiKey);

    public string GenerateBattleStartCommentary(Player attacker, Player? defender, Country attackerCountry, Country defenderCountry)
        => CommentaryEngine.GenerateBattleStartCommentary(attacker, defender, attackerCountry, defenderCountry);

    public string GenerateClashCommentary(List<DicePairing> pairings, Country attackerCountry, Country defenderCountry, int attackerLosses, int defenderLosses)
        => CommentaryEngine.GenerateClashCommentary(pairings, attackerCountry, defenderCountry, attackerLosses, defenderLosses);

    public string GenerateConquestCommentary(Player attacker, Player? defender, Country conquered, int bonus, bool defenderEliminated)
        => CommentaryEngine.GenerateConquestCommentary(attacker, defender, conquered, bonus, defenderEliminated);

    public string GenerateRetreatCommentary(Player attacker, Country attackerCountry, Country defenderCountry)
        => CommentaryEngine.GenerateRetreatCommentary(attacker, attackerCountry, defenderCountry);

    public string GenerateDefeatCommentary(Player attacker, Country attackerCountry, Country defenderCountry)
        => CommentaryEngine.GenerateDefeatCommentary(attacker, attackerCountry, defenderCountry);
}

