using System.Text.RegularExpressions;

namespace PlanningConverter.Parsing.Normalization;

public readonly record struct TimeParseResult(TimeOnly? Value, bool Success, string? Error);

/// <summary>
/// Reconnaît un horaire isolé sous ses formes courantes (§18) :
/// 08:30, 8:30, 08h30, 8h30, 8 H 30, 0830, 14h (minutes sous-entendues à 00).
/// Grammaire ordonnée : la première correspondance gagne.
/// </summary>
public static partial class TimeParser
{
    [GeneratedRegex(@"^(?<h>\d{1,2})\s*[:hH.]\s*(?<m>\d{2})$")]
    private static partial Regex ColonOrLetterSeparated();

    [GeneratedRegex(@"^(?<h>\d{1,2})$")]
    private static partial Regex HourOnly();

    [GeneratedRegex(@"^(?<hm>\d{4})$")]
    private static partial Regex FourDigits();

    [GeneratedRegex(@"^(?<h>\d{1,2})\s*[hH]$")]
    private static partial Regex HourWithSuffixLetter();

    public static TimeParseResult Parse(string raw)
    {
        var s = raw.Trim();
        if (s.Length == 0) return new TimeParseResult(null, false, "vide");

        var m = ColonOrLetterSeparated().Match(s);
        if (m.Success)
        {
            return Build(m.Groups["h"].Value, m.Groups["m"].Value, s);
        }

        m = HourWithSuffixLetter().Match(s);
        if (m.Success)
        {
            return Build(m.Groups["h"].Value, "00", s);
        }

        m = FourDigits().Match(s);
        if (m.Success)
        {
            var digits = m.Groups["hm"].Value;
            return Build(digits[..2], digits[2..], s);
        }

        return new TimeParseResult(null, false, $"format d'heure non reconnu : '{raw}'");
    }

    private static TimeParseResult Build(string hourText, string minuteText, string original)
    {
        if (!int.TryParse(hourText, out var h) || !int.TryParse(minuteText, out var min))
        {
            return new TimeParseResult(null, false, $"heure invalide : '{original}'");
        }
        if (h is < 0 or > 23 || min is < 0 or > 59)
        {
            return new TimeParseResult(null, false, $"heure hors bornes : '{original}'");
        }
        return new TimeParseResult(new TimeOnly(h, min), true, null);
    }
}
