using System.Text.RegularExpressions;

namespace PlanningConverter.Parsing.Lexicons;

/// <summary>
/// §96. Sur les fichiers réels observés, la promotion apparaît en toutes lettres
/// dans la première ligne : "PROMOTION MK4". On extrait le code, pas la phrase entière.
/// </summary>
public static partial class PromotionLexicon
{
    [GeneratedRegex(@"\b(MK|BUT|DUT)\s?[1-5]\b|\b[LM][1-3]\b", RegexOptions.IgnoreCase)]
    private static partial Regex CodeRegex();

    public static string? Extract(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var m = CodeRegex().Match(text);
        return m.Success ? m.Value.ToUpperInvariant().Replace(" ", "") : null;
    }
}
