using System.Text;
using System.Text.Json;
using Geopolitics.Core.Models;

namespace Geopolitics.Core.Engines;

public static class CommentaryEngine
{
    private static readonly HttpClient Http = new();

    private static readonly string[] Headlines =
    {
        "GLOBAL DEFCON ALERT: Geopolitical equilibrium shattered as armies mobilize!",
        "WAR BULLETIN: Satellites detect massive troop movements across international borders!",
        "STRATEGIC UPDATE: High Command issues emergency readiness directives worldwide!",
        "BREAKING: Continental alliances fragment under sustained territorial aggression!"
    };

    private static readonly string[] AttackerWinQuotes =
    {
        "Surgical bombardment! Attacker's artillery rolls high and obliterates enemy vanguard!",
        "Decisive breakthrough! Infantry breaches the trenchline with overwhelming firepower!",
        "A masterstroke of armored doctrine! Attacker claims victory on this skirmish flank!",
        "Air superiority confirmed! Defender takes catastrophic losses as the dice roll in the attacker's favor!"
    };

    private static readonly string[] DefenderWinQuotes =
    {
        "Ironclad resistance! The defending garrison holds the ridge with dogged determination!",
        "Ambush sprung! Defensive counter-battery fire decimates the advancing column!",
        "Fortifications hold! Attacking units fall back under blistering machine-gun crossfire!",
        "A heroic stand! Defender rolls the higher die, repelling the invaders with fierce valor!"
    };

    private static readonly string[] TieQuotes =
    {
        "Deadlock in the mud! Both forces roll identical dice — stalemate persists without casualties!",
        "Heavy artillery duel ends in an eerie silence. Neither side secures the advantage!",
        "Tactical standoff: both commanders regroup without yielding a single meter of terrain."
    };

    private static readonly string[] ConquestDispatches =
    {
        "HISTORIC FALL: The defending citadel has surrendered! Sovereignty officially transfers!",
        "MAP REDRAWN: Victory parades sweep the streets as the conqueror assumes total administrative control!",
        "GEOPOLITICAL EARTHQUAKE: The nation falls! Victorious forces collect massive recruitment dividends!",
        "FLAG LOWERED: A new era begins as the triumphant empire installs military governors!"
    };

    private static string PickRandom(string[] array) => array[GameEngine.Rng.Next(array.Length)];

    public static string GenerateClashCommentary(
        List<DicePairing> pairings,
        Country attackerCountry,
        Country defenderCountry,
        int attackerLosses,
        int defenderLosses)
    {
        var narrativeParts = new List<string>();

        for (int i = 0; i < pairings.Count; i++)
        {
            var pair = pairings[i];
            int soldierNum = i + 1;
            string quote = pair.Outcome switch
            {
                "attacker_won" => PickRandom(AttackerWinQuotes),
                "defender_won" => PickRandom(DefenderWinQuotes),
                _ => PickRandom(TieQuotes)
            };

            narrativeParts.Add($"Squadron {soldierNum} [{pair.AttackerDie} vs {pair.DefenderDie}]: {quote}");
        }

        string casualtySummary = $"Casualty report: {attackerCountry.Name} -{attackerLosses} troops ({attackerCountry.Soldiers} remain) | {defenderCountry.Name} -{defenderLosses} troops ({defenderCountry.Soldiers} remain).";
        narrativeParts.Add(casualtySummary);

        return string.Join(" ", narrativeParts);
    }

    public static string GenerateBattleStartCommentary(
        Player attackerPlayer,
        Player? defenderPlayer,
        Country attackerCountry,
        Country defenderCountry)
    {
        string headline = PickRandom(Headlines);
        return $"⚔️ {headline} {attackerPlayer.Name} has ordered {attackerCountry.Name} ({attackerCountry.Soldiers} troops) to invade {defenderCountry.Name} ({defenderCountry.Soldiers} troops)! The dice of destiny are cast!";
    }

    public static string GenerateConquestCommentary(
        Player attackerPlayer,
        Player? defenderPlayer,
        Country conqueredCountry,
        int conquerBonus,
        bool defenderEliminated)
    {
        string dispatch = PickRandom(ConquestDispatches);
        string extra = defenderEliminated
            ? $" 💀 FACTION COLLAPSE: {defenderPlayer?.Name ?? "The defending faction"} has lost their final sovereign territory and is ELIMINATED from the game!"
            : string.Empty;

        return $"🏆 {dispatch} {attackerPlayer.Name} has officially conquered {conqueredCountry.Name}! Gained +{conquerBonus} veteran soldiers from the annexation!{extra}";
    }

    public static string GenerateRetreatCommentary(Player attackerPlayer, Country attackerCountry, Country defenderCountry)
    {
        return $"🛡️ TACTICAL CEASEFIRE: {attackerPlayer.Name} breaks off the assault on ${defenderCountry.Name}. Surviving troops fall back to regroup in {attackerCountry.Name}.";
    }

    public static string GenerateDefeatCommentary(Player attackerPlayer, Country attackerCountry, Country defenderCountry)
    {
        return $"💥 INVASION CRUSHED: {attackerCountry.Name}'s assault force was ground down to a single soldier. {defenderCountry.Name} stands defiant!";
    }

    public static async Task<string?> GenerateGeminiCommentaryAsync(string prompt, string? apiKey = null)
    {
        var key = apiKey ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(key)) return null;

        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={key}";
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = $"You are an energetic, dramatic, high-stakes military and geopolitical news anchor covering a live war game. Keep your response concise (1-2 sentences), punchy, and atmospheric.\n\nSituation: {prompt}"
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = 80,
                    temperature = 0.8
                }
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Http.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var respBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(respBody);
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0 &&
                    candidates[0].TryGetProperty("content", out var cObj) &&
                    cObj.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0 &&
                    parts[0].TryGetProperty("text", out var textEl))
                {
                    return textEl.GetString()?.Trim();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gemini API commentary notice: {ex.Message}");
        }

        return null;
    }

    public static async Task<CommentaryEntry> AddCommentaryToGameAsync(
        GameState game,
        string text,
        string type = "battle",
        string? customApiKey = null)
    {
        string finalText = text;

        if (!string.IsNullOrEmpty(customApiKey) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GEMINI_API_KEY")))
        {
            var aiText = await GenerateGeminiCommentaryAsync(text, customApiKey);
            if (!string.IsNullOrEmpty(aiText))
            {
                finalText = $"[AI Dispatch] {aiText}";
            }
        }

        var entry = new CommentaryEntry
        {
            Id = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow.ToString("o"),
            Text = finalText,
            Type = type
        };

        game.RecentCommentary.Insert(0, entry);
        if (game.RecentCommentary.Count > 50)
        {
            game.RecentCommentary = game.RecentCommentary.Take(50).ToList();
        }

        return entry;
    }
}

