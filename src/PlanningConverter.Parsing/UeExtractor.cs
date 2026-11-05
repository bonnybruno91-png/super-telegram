using System.Text.RegularExpressions;

namespace PlanningConverter.Parsing;

public sealed record UeSplitResult(string? Ue, string CleanTitle);

/// <summary>
/// Sur les fichiers réels, l'UE n'a pas de colonne dédiée : elle est préfixée à l'intitulé
/// ("UE18 ORAL PATHOLOGIES SANTE MENTALE", "UI25 INFORMATION ECOS"). On la détache pour
/// alimenter le champ <c>Ue</c> du modèle, sans jamais perdre l'information d'origine
/// (elle reste consultable via SourceCells / OriginalContent, §94).
/// </summary>
public static partial class UeExtractor
{
    [GeneratedRegex(@"^(?<code>UE|UI)\s?(?<num>\d{1,3})\b\s*(?<rest>.*)$", RegexOptions.IgnoreCase)]
    private static partial Regex Pattern();

    public static UeSplitResult Split(string? rawTitle)
    {
        if (string.IsNullOrWhiteSpace(rawTitle))
        {
            return new UeSplitResult(null, "");
        }

        var s = rawTitle.Trim();
        var m = Pattern().Match(s);
        if (!m.Success)
        {
            return new UeSplitResult(null, s);
        }

        var ue = $"{m.Groups["code"].Value.ToUpperInvariant()}{m.Groups["num"].Value}";
        var rest = m.Groups["rest"].Value.Trim();
        return new UeSplitResult(ue, rest.Length > 0 ? rest : s);
    }
}
