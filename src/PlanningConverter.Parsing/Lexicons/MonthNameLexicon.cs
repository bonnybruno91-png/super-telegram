namespace PlanningConverter.Parsing.Lexicons;

/// <summary>Reconnaissance des noms de mois français, avec et sans accents (§17).</summary>
public static class MonthNameLexicon
{
    private static readonly Dictionary<string, int> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["janvier"] = 1, ["février"] = 2, ["fevrier"] = 2, ["mars"] = 3, ["avril"] = 4,
        ["mai"] = 5, ["juin"] = 6, ["juillet"] = 7, ["août"] = 8, ["aout"] = 8,
        ["septembre"] = 9, ["octobre"] = 10, ["novembre"] = 11, ["décembre"] = 12, ["decembre"] = 12,
    };

    public static bool TryParse(string text, out int month) => Map.TryGetValue(text.Trim(), out month);
}
