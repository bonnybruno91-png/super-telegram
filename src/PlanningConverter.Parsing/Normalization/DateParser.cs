using System.Text.RegularExpressions;
using PlanningConverter.Parsing.Lexicons;

namespace PlanningConverter.Parsing.Normalization;

public sealed record DayHeaderResult(DateOnly Date, DayOfWeek DeclaredDay, bool WeekdayCoherent);

/// <summary>
/// §17, calibré sur le format d'en-tête de bloc jour confirmé par les fichiers réels :
/// "Lundi 01 Juin 2026" — nom du jour complet, numéro, mois en toutes lettres, année à 4 chiffres.
/// Complété par des formats génériques pour rester compatible avec d'autres établissements.
/// </summary>
public static partial class DateParser
{
    [GeneratedRegex(
        @"^(?<day>lundi|mardi|mercredi|jeudi|vendredi|samedi|dimanche)\s+" +
        @"(?<num>\d{1,2})(?:er)?\s+(?<month>[a-zéèûôâîùç]+)\s+(?<year>\d{4})$",
        RegexOptions.IgnoreCase)]
    private static partial Regex FullDayHeaderPattern();

    [GeneratedRegex(@"^(?<d>\d{1,2})[/\-.](?<m>\d{1,2})([/\-.](?<y>\d{2,4}))?$")]
    private static partial Regex NumericDatePattern();

    [GeneratedRegex(@"^(?<d>\d{1,2})(?:er)?\s+(?<month>[a-zéèûôâîùç]+)(\s+(?<y>\d{4}))?$", RegexOptions.IgnoreCase)]
    private static partial Regex TextualDatePattern();

    /// <summary>
    /// Reconnaît "Lundi 01 Juin 2026" et vérifie la cohérence entre le nom du jour annoncé
    /// et le jour réel de la date grégorienne — un contrôle qualité gratuit sur le fichier
    /// source (§55 : dates incohérentes), qui n'existait pas explicitement dans le §17.
    /// </summary>
    public static DayHeaderResult? TryParseDayHeader(string text)
    {
        var m = FullDayHeaderPattern().Match(text.Trim());
        if (!m.Success) return null;

        if (!DayNameLexicon.TryParse(m.Groups["day"].Value, out var declaredDay)) return null;
        if (!MonthNameLexicon.TryParse(m.Groups["month"].Value, out var month)) return null;
        if (!int.TryParse(m.Groups["num"].Value, out var day)) return null;
        if (!int.TryParse(m.Groups["year"].Value, out var year)) return null;

        DateOnly date;
        try { date = new DateOnly(year, month, day); }
        catch (ArgumentOutOfRangeException) { return null; }

        var coherent = date.DayOfWeek == declaredDay;
        return new DayHeaderResult(date, declaredDay, coherent);
    }

    /// <summary>
    /// Formats génériques §17 : 14/09/2026, 14-09-2026, 14.09.2026, 14 septembre 2026, 14/09.
    /// <paramref name="defaultYear"/> sert quand l'année est absente de la cellule (ancrage de semaine,
    /// plan §6.4) — jamais deviné sans ce paramètre explicite.
    /// </summary>
    public static DateOnly? TryParseGenericDate(string text, int? defaultYear = null)
    {
        var s = text.Trim();

        var m = NumericDatePattern().Match(s);
        if (m.Success)
        {
            var day = int.Parse(m.Groups["d"].Value);
            var month = int.Parse(m.Groups["m"].Value);
            var yearText = m.Groups["y"].Success ? m.Groups["y"].Value : null;
            var year = yearText is not null
                ? (yearText.Length == 2 ? 2000 + int.Parse(yearText) : int.Parse(yearText))
                : defaultYear;
            if (year is null) return null;
            try { return new DateOnly(year.Value, month, day); }
            catch (ArgumentOutOfRangeException) { return null; }
        }

        m = TextualDatePattern().Match(s);
        if (m.Success && MonthNameLexicon.TryParse(m.Groups["month"].Value, out var monthNum))
        {
            var day = int.Parse(m.Groups["d"].Value);
            var year = m.Groups["y"].Success ? int.Parse(m.Groups["y"].Value) : defaultYear;
            if (year is null) return null;
            try { return new DateOnly(year.Value, monthNum, day); }
            catch (ArgumentOutOfRangeException) { return null; }
        }

        return null;
    }
}
