using System.Globalization;
using System.Text;
using PlanningConverter.Domain;

namespace PlanningConverter.Export;

/// <summary>
/// Écriture CSV minimale (RFC 4180) : champs entre guillemets si nécessaire, guillemets doublés,
/// fins de ligne CRLF. Les champs textuels issus du classeur sont protégés contre l'évaluation
/// de formule par un tableur (cahier §8.3) : un texte commençant par = + - @ tabulation ou
/// retour chariot reçoit une apostrophe de tête.
/// </summary>
public static class CsvWriterLite
{
    public static string Protect(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text[0] is '=' or '+' or '-' or '@' or '\t' or '\r' ? "'" + text : text;
    }

    public static string Quote(string field, char delimiter)
    {
        var needsQuotes = field.IndexOfAny([delimiter, '"', '\r', '\n']) >= 0;
        return needsQuotes ? "\"" + field.Replace("\"", "\"\"") + "\"" : field;
    }

    public static string Row(IEnumerable<string> fields, char delimiter) =>
        string.Join(delimiter, fields.Select(f => Quote(f, delimiter))) + "\r\n";

    /// <summary>Compte les enregistrements (hors en-tête) en respectant les retours à la ligne entre guillemets.</summary>
    public static int CountRecords(string content)
    {
        var records = 0;
        var inQuotes = false;
        var lineHasContent = false;
        for (var i = 0; i < content.Length; i++)
        {
            var c = content[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < content.Length && content[i + 1] == '"') { i++; continue; }
                inQuotes = !inQuotes;
                lineHasContent = true;
            }
            else if (c == '\n' && !inQuotes)
            {
                if (lineHasContent) records++;
                lineHasContent = false;
            }
            else if (c != '\r')
            {
                lineHasContent = true;
            }
        }
        if (lineHasContent) records++;
        return Math.Max(0, records - 1); // moins l'en-tête
    }
}

/// <summary>
/// Export CSV (§40), deux profils :
/// - <see cref="CsvProfile.Generic"/> : point-virgule, dates AAAA-MM-JJ, UTF-8 avec BOM (Excel français) ;
/// - <see cref="CsvProfile.GoogleCalendar"/> : virgule, dates MM/JJ/AAAA (contrainte de l'importateur
///   Google), UTF-8 sans BOM, colonnes attendues par l'import d'événements.
/// </summary>
public sealed class CsvExporter : IExporter
{
    public string Id => "csv";
    public string DisplayName => "CSV";

    public async Task<ExportedFile> ExportAsync(ExportRequest request, CancellationToken ct = default)
    {
        var exportable = ExportSelection.Publishable(request.Events);

        Directory.CreateDirectory(request.OutputFolder);
        var baseName = request.BaseName ?? FileNaming.BuildBaseName(request.Promotion, request.PeriodStart,
            request.PeriodEnd, request.GroupLabel, request.GeneratedAt);
        var path = FileNaming.ResolveNonColliding(request.OutputFolder, baseName, "csv");

        var google = request.CsvProfile == CsvProfile.GoogleCalendar;
        var delimiter = google ? ',' : ';';
        var dateFormat = google ? "MM/dd/yyyy" : "yyyy-MM-dd";

        var sb = new StringBuilder();
        sb.Append(CsvWriterLite.Row(Header(google), delimiter));
        foreach (var ev in exportable)
        {
            sb.Append(CsvWriterLite.Row(Fields(ev, google, dateFormat, request.TitleFormat), delimiter));
        }

        await AtomicFile.WriteTextAsync(path, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: !google), ct);
        return await VerifyAndDescribe(path, exportable.Count, ct);
    }

    private static IEnumerable<string> Header(bool google) => google
        ? ["Subject", "Start Date", "Start Time", "End Date", "End Time", "All Day Event", "Description", "Location", "Private"]
        : ["Date", "Jour", "Heure de début", "Heure de fin", "Journée entière", "Cours", "Enseignant", "Salle", "UE", "Groupe", "Notes"];

    private static IEnumerable<string> Fields(ScheduleEvent ev, bool google, string dateFormat, TitleFormat titleFormat)
    {
        var fr = CultureInfo.GetCultureInfo("fr-FR");
        var dateText = ev.Date?.ToString(dateFormat, CultureInfo.InvariantCulture) ?? "";

        if (google)
        {
            return
            [
                CsvWriterLite.Protect(TitleBuilder.Build(ev, titleFormat)),
                dateText,
                ev.IsAllDay ? "" : ev.StartTime?.ToString("hh:mm tt", CultureInfo.InvariantCulture) ?? "",
                dateText,
                ev.IsAllDay ? "" : ev.EndTime?.ToString("hh:mm tt", CultureInfo.InvariantCulture) ?? "",
                ev.IsAllDay ? "True" : "False",
                CsvWriterLite.Protect(DescriptionBuilder.Build(ev)),
                CsvWriterLite.Protect(ev.Room ?? ev.Location),
                "False",
            ];
        }

        var dayText = ev.Date is { } d ? fr.TextInfo.ToTitleCase(d.ToString("dddd", fr)) : "";
        return
        [
            dateText,
            dayText,
            ev.IsAllDay ? "" : ev.StartTime?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? "",
            ev.IsAllDay ? "" : ev.EndTime?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? "",
            ev.IsAllDay ? "Oui" : "Non",
            CsvWriterLite.Protect(ev.Title),
            CsvWriterLite.Protect(ev.Teacher),
            CsvWriterLite.Protect(ev.Room ?? ev.Location),
            CsvWriterLite.Protect(ev.Ue),
            CsvWriterLite.Protect(ev.Group),
            CsvWriterLite.Protect(ev.Notes),
        ];
    }

    private static async Task<ExportedFile> VerifyAndDescribe(string path, int expectedCount, CancellationToken ct)
    {
        if (!File.Exists(path)) throw new IOException($"Le fichier CSV n'a pas été créé : {path}");

        var content = await File.ReadAllTextAsync(path, ct);
        var actual = CsvWriterLite.CountRecords(content);
        if (actual != expectedCount)
        {
            throw new IOException($"Le fichier CSV contient {actual} ligne(s) de données, {expectedCount} attendue(s) : {path}");
        }

        return new ExportedFile(path, new FileInfo(path).Length, actual);
    }
}
