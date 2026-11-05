using System.Globalization;
using System.Text;

namespace PlanningConverter.Parsing.Lexicons;

public enum SemanticColumn { Day, Date, Time, Title, Teacher, Room }

/// <summary>
/// §21 : le moteur doit reconnaître des synonymes d'en-tête. Externalisé sous forme de
/// table plutôt que codé en dur dans le moteur, pour que le secrétariat puisse
/// l'enrichir un jour sans recompilation (voir plan §5, étage 5).
/// </summary>
public static class HeaderSynonymLexicon
{
    private static readonly Dictionary<SemanticColumn, string[]> Synonyms = new()
    {
        [SemanticColumn.Title] = ["cours", "matiere", "intitule", "enseignement", "module", "discipline"],
        [SemanticColumn.Teacher] = ["enseignant", "professeur", "intervenant", "formateur", "prof"],
        [SemanticColumn.Room] = ["salle", "lieu", "local", "amphi", "amphitheatre"],
        [SemanticColumn.Time] = ["horaire", "horaires", "heure", "creneau"],
        [SemanticColumn.Day] = ["jour", "journee"],
        [SemanticColumn.Date] = ["date"],
    };

    /// <summary>Compare après normalisation (minuscules, accents retirés) — "Intitulé" == "intitule".</summary>
    public static SemanticColumn? Match(string headerText)
    {
        var normalized = Normalize(headerText);
        foreach (var (column, words) in Synonyms)
        {
            if (words.Any(w => Normalize(w) == normalized))
            {
                return column;
            }
        }
        return null;
    }

    public static string Normalize(string text)
    {
        var form = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in form)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
