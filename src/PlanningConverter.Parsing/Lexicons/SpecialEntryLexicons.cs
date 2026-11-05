using System.Text.RegularExpressions;

namespace PlanningConverter.Parsing.Lexicons;

/// <summary>
/// Recherche d'un mot-clé comme MOT ENTIER dans un texte déjà normalisé (minuscules, sans accents).
/// Une simple sous-chaîne produisait de faux positifs : « conges » dans « congestion »,
/// « pont » dans « ponction », « repas » dans « repassage »…
/// </summary>
internal static class WordMatch
{
    public static bool ContainsWord(string normalizedText, string keyword)
    {
        var pattern = @"(?<![\p{L}\p{N}])" + Regex.Escape(keyword) + @"(?![\p{L}\p{N}])";
        return Regex.IsMatch(normalizedText, pattern);
    }

    public static bool ContainsAny(string? text, IEnumerable<string> keywords)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var normalized = HeaderSynonymLexicon.Normalize(text);
        return keywords.Any(k => ContainsWord(normalized, k));
    }
}

/// <summary>§23 : entrées qui ne doivent pas devenir des événements calendrier (pauses déjeuner).</summary>
public static class BreakLexicon
{
    private static readonly string[] Keywords = ["pause dejeuner", "pause", "repas", "dejeuner"];

    public static bool IsBreak(string? title) => WordMatch.ContainsAny(title, Keywords);
}

/// <summary>
/// §57 : une entrée sans aucune heure peut légitimement être un événement pleine journée
/// (jour férié, fermeture) — mais seulement si le texte le dit clairement. Liste volontairement
/// restreinte ; hors liste, l'absence d'heure reste une anomalie bloquante à faire trancher.
/// </summary>
public static class AllDayHintLexicon
{
    private static readonly string[] Keywords =
    [
        "ferie", "fermeture", "ferme", "vacances", "conges",
        "victoire", "armistice", "toussaint", "noel", "jour de l'an", "ascension",
        "pentecote", "assomption", "fete du travail", "fete nationale", "pont"
    ];

    public static bool LooksLikeAllDayEvent(string? title) => WordMatch.ContainsAny(title, Keywords);
}
