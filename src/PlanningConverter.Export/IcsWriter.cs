using System.Globalization;
using System.Text;

namespace PlanningConverter.Export;

/// <summary>
/// Écriture minimale et vérifiable de texte iCalendar (RFC 5545) : échappement des valeurs
/// TEXT, pliage des lignes à 75 octets UTF-8 sans couper un caractère, fins de ligne CRLF.
/// </summary>
public static class IcsWriter
{
    private const int MaxOctets = 75;

    /// <summary>Échappe une valeur de type TEXT : \, ;, , et retours à la ligne.</summary>
    public static string EscapeText(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        var sb = new StringBuilder(text.Length + 8);
        foreach (var ch in text)
        {
            switch (ch)
            {
                case '\\': sb.Append("\\\\"); break;
                case ';': sb.Append("\\;"); break;
                case ',': sb.Append("\\,"); break;
                case '\r': break;
                case '\n': sb.Append("\\n"); break;
                default: sb.Append(ch); break;
            }
        }
        return sb.ToString();
    }

    /// <summary>Plie une ligne logique en lignes physiques de 75 octets maximum (suite = espace).</summary>
    public static string Fold(string line)
    {
        if (Encoding.UTF8.GetByteCount(line) <= MaxOctets) return line + "\r\n";

        var sb = new StringBuilder();
        var current = new StringBuilder();
        var octets = 0;
        var limit = MaxOctets;

        var enumerator = StringInfo.GetTextElementEnumerator(line);
        while (enumerator.MoveNext())
        {
            var element = (string)enumerator.Current;
            var size = Encoding.UTF8.GetByteCount(element);
            if (octets + size > limit)
            {
                sb.Append(current).Append("\r\n");
                current.Clear();
                current.Append(' ');
                octets = 1;
                limit = MaxOctets;
            }
            current.Append(element);
            octets += size;
        }
        sb.Append(current).Append("\r\n");
        return sb.ToString();
    }

    public static string FormatLocal(DateOnly date, TimeOnly time) =>
        date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "T" + time.ToString("HHmmss", CultureInfo.InvariantCulture);

    public static string FormatDate(DateOnly date) => date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

    public static string FormatUtc(DateTimeOffset moment) =>
        moment.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
}
