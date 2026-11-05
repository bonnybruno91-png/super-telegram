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

/// <summary>
/// §97. Sur les fichiers réels, un code de groupe apparaît parfois dans la colonne "Salle",
/// mêlé au nom du lieu ou seul sur sa propre ligne (ex. "P1 IFMKDV", "S3 IFMKDV").
/// Reconnaissance volontairement prudente : mieux vaut ne pas extraire que mal extraire (I1).
/// </summary>
public static partial class GroupCodeLexicon
{
    [GeneratedRegex(@"^[PSGL]\d{1,2}(\s+\S+)?$", RegexOptions.IgnoreCase)]
    private static partial Regex GroupLinePattern();

    private static readonly string[] RoomKeywords =
        ["AMPHI", "SALLE", "LOCAL", "BUREAU", "LABO", "GYMNASE", "PLATEAU", "ATELIER"];

    /// <summary>Une ligne isolée ressemble-t-elle à un code de groupe plutôt qu'à un nom de lieu ?</summary>
    public static bool LooksLikeGroupCode(string line)
    {
        var upper = line.Trim().ToUpperInvariant();
        if (RoomKeywords.Any(k => upper.Contains(k))) return false;
        return GroupLinePattern().IsMatch(line.Trim());
    }
}
