using System.Text.RegularExpressions;

namespace PlanningConverter.Parsing;

/// <summary>
/// Colonne « Groupe » du secrétariat : « GROUPE  B », « Groupe A », « B », « A et B ».
/// Résultat normalisé « Groupe B » (plusieurs : « Groupe A, Groupe B »). Cellule vide = séance commune.
/// Un texte qui ne ressemble à aucun code de groupe est conservé tel quel (rien n'est inventé).
/// </summary>
public static class GroupLabel
{
    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var text = Regex.Replace(raw.Trim(), @"\s+", " ");
        var withoutWord = Regex.Replace(text, @"(?i)\bgroupes?\b|\bgr\.?(?=\s|$)", " ");
        var tokens = Regex.Split(withoutWord, @"(?i)[\s,;/&+]+|\bet\b")
            .Where(t => t.Length > 0).ToList();

        if (tokens.Count > 0 && tokens.All(t => t.Length == 1 && char.IsLetterOrDigit(t[0])))
        {
            return string.Join(", ", tokens.Select(t => $"Groupe {t.ToUpperInvariant()}").Distinct());
        }
        return text;
    }
}
