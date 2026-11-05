using System.Text;
using PlanningConverter.Domain;

namespace PlanningConverter.Export;

/// <summary>
/// Export ICS / iCalendar (cahier §8.1). Écrit sans bibliothèque externe pour maîtriser le
/// contenu (voir PlanningConverter.Export.csproj) :
/// - VTIMEZONE Europe/Paris complet (<see cref="ParisTimeZoneBlock"/>) ;
/// - UID stable fourni par l'événement, DTSTAMP/LAST-MODIFIED/SEQUENCE explicites ;
/// - UTF-8 sans BOM, CRLF, lignes pliées à 75 octets ;
/// - une séance incomplète (anomalie bloquante) est exclue, jamais l'export entier (VER-03) ;
/// - le fichier écrit est relu et contrôlé avant de déclarer le succès.
/// </summary>
public sealed class IcsExporter : IExporter
{
    public string Id => "ics";
    public string DisplayName => "Calendrier ICS";

    public async Task<ExportedFile> ExportAsync(ExportRequest request, CancellationToken ct = default)
    {
        var exportable = ExportSelection.Publishable(request.Events);
        var content = Build(exportable, request);

        Directory.CreateDirectory(request.OutputFolder);
        var baseName = request.BaseName ?? FileNaming.BuildBaseName(request.Promotion, request.PeriodStart,
            request.PeriodEnd, request.GroupLabel, request.GeneratedAt);
        var path = FileNaming.ResolveNonColliding(request.OutputFolder, baseName, "ics");

        await AtomicFile.WriteTextAsync(path, content, new UTF8Encoding(false), ct);
        return await VerifyAndDescribe(path, exportable.Count, ct);
    }

    public static string Build(IReadOnlyList<ScheduleEvent> events, ExportRequest request)
    {
        var stamp = IcsWriter.FormatUtc(request.GeneratedAt ?? DateTimeOffset.Now);
        var sb = new StringBuilder();
        void Line(string l) => sb.Append(IcsWriter.Fold(l));

        Line("BEGIN:VCALENDAR");
        Line("VERSION:2.0");
        Line("PRODID:-//Planning Converter//FR");
        Line("CALSCALE:GREGORIAN");
        Line("METHOD:PUBLISH");
        var label = string.IsNullOrWhiteSpace(request.GroupLabel) ? request.Promotion : $"{request.Promotion} {request.GroupLabel}";
        Line("X-WR-CALNAME:" + IcsWriter.EscapeText(
            $"Planning {label} du {request.PeriodStart:dd/MM/yyyy} au {request.PeriodEnd:dd/MM/yyyy}"));
        Line("X-WR-TIMEZONE:Europe/Paris");
        sb.Append(ParisTimeZoneBlock.Text);

        foreach (var ev in events)
        {
            AppendEvent(sb, ev, request, stamp);
        }

        Line("END:VCALENDAR");
        return sb.ToString();
    }

    private static void AppendEvent(StringBuilder sb, ScheduleEvent ev, ExportRequest request, string stamp)
    {
        void Line(string l) => sb.Append(IcsWriter.Fold(l));

        Line("BEGIN:VEVENT");
        Line("UID:" + ev.Uid);
        Line("DTSTAMP:" + stamp);
        Line("LAST-MODIFIED:" + stamp);
        Line("SEQUENCE:" + Math.Max(0, ev.Revision));

        if (ev.IsAllDay && ev.Date is { } d)
        {
            Line("DTSTART;VALUE=DATE:" + IcsWriter.FormatDate(d));
            Line("DTEND;VALUE=DATE:" + IcsWriter.FormatDate(d.AddDays(1)));
            Line("TRANSP:TRANSPARENT");
        }
        else if (ev.Date is { } date && ev.StartTime is { } start && ev.EndTime is { } end)
        {
            Line("DTSTART;TZID=Europe/Paris:" + IcsWriter.FormatLocal(date, start));
            Line("DTEND;TZID=Europe/Paris:" + IcsWriter.FormatLocal(date, end));
        }
        else
        {
            throw new InvalidOperationException(
                $"Événement '{ev.Title}' sans date/heure exploitable malgré la sélection préalable.");
        }

        Line("SUMMARY:" + IcsWriter.EscapeText(TitleBuilder.Build(ev, request.TitleFormat)));
        var location = ev.Room ?? ev.Location;
        if (!string.IsNullOrWhiteSpace(location)) Line("LOCATION:" + IcsWriter.EscapeText(location));
        var description = DescriptionBuilder.Build(ev);
        if (description.Length > 0) Line("DESCRIPTION:" + IcsWriter.EscapeText(description));
        Line("END:VEVENT");
    }

    private static async Task<ExportedFile> VerifyAndDescribe(string path, int expectedCount, CancellationToken ct)
    {
        if (!File.Exists(path)) throw new IOException($"Le fichier ICS n'a pas été créé : {path}");

        var content = await File.ReadAllTextAsync(path, ct);
        if (!content.StartsWith("BEGIN:VCALENDAR\r\n", StringComparison.Ordinal) ||
            !content.EndsWith("END:VCALENDAR\r\n", StringComparison.Ordinal))
        {
            throw new IOException($"Le fichier ICS généré est mal formé : {path}");
        }

        var actual = content.Split("\r\nBEGIN:VEVENT\r\n").Length - 1;
        if (actual != expectedCount)
        {
            throw new IOException($"Le fichier ICS contient {actual} événement(s), {expectedCount} attendu(s) : {path}");
        }

        return new ExportedFile(path, new FileInfo(path).Length, actual);
    }
}
