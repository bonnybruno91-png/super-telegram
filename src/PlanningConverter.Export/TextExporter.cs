using System.Globalization;
using System.Text;
using PlanningConverter.Domain;

namespace PlanningConverter.Export;

/// <summary>
/// Construit la représentation texte du planning, dans les deux formats du §41.
/// Partagée par <see cref="TextExporter"/>, la future action presse-papiers (§42) et la
/// préparation d'e-mail (§46) — un seul endroit décide de la mise en forme.
/// </summary>
public static class PlanningTextFormatter
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    public static string Format(IReadOnlyList<ScheduleEvent> events, TextFormat format)
    {
        var exportable = ExportSelection.Publishable(events)
            .Where(e => e.Date is not null)
            .GroupBy(e => e.Date!.Value);

        var sb = new StringBuilder();
        var first = true;

        foreach (var dayGroup in exportable)
        {
            if (!first) sb.Append('\n');
            first = false;

            sb.Append(format == TextFormat.Detailed
                ? CapitalizeFirst(dayGroup.Key.ToString("dddd d MMMM yyyy", Fr))
                : CapitalizeFirst(dayGroup.Key.ToString("dddd d MMMM", Fr)));
            sb.Append('\n');

            if (format == TextFormat.Detailed) sb.Append('\n');

            foreach (var ev in dayGroup)
            {
                sb.Append(format == TextFormat.Detailed ? FormatDetailed(ev) : FormatCompact(ev));
            }
        }

        return sb.ToString().TrimEnd('\n') + "\n";
    }

    private static string FormatDetailed(ScheduleEvent ev)
    {
        var sb = new StringBuilder();
        sb.Append($"{TimeRange(ev)}\n");
        sb.Append($"{TitleBuilder.ExactTitle(ev)}{GroupSuffix(ev)}\n");
        if (!string.IsNullOrWhiteSpace(ev.Teacher)) sb.Append($"Enseignant : {ev.Teacher}\n");
        if (!string.IsNullOrWhiteSpace(ev.Room)) sb.Append($"Salle : {ev.Room}\n");
        sb.Append('\n');
        return sb.ToString();
    }

    private static string FormatCompact(ScheduleEvent ev)
    {
        var parts = new List<string> { TitleBuilder.ExactTitle(ev) + GroupSuffix(ev) };
        if (!string.IsNullOrWhiteSpace(ev.Teacher)) parts.Add(ev.Teacher);
        if (!string.IsNullOrWhiteSpace(ev.Room)) parts.Add($"Salle {ev.Room}");
        return $"{TimeRangeCompact(ev)} : {string.Join(" — ", parts)}\n";
    }

    private static string GroupSuffix(ScheduleEvent ev) =>
        string.IsNullOrWhiteSpace(ev.Group) ? "" : $" — {ev.Group}";

    private static string TimeRange(ScheduleEvent ev) =>
        ev.IsAllDay ? "Journée entière" : $"{ev.StartTime:HH\\hmm} - {ev.EndTime:HH\\hmm}";

    private static string TimeRangeCompact(ScheduleEvent ev) =>
        ev.IsAllDay ? "Journée entière" : $"{ev.StartTime:HH\\hmm}-{ev.EndTime:HH\\hmm}";

    private static string CapitalizeFirst(string s) =>
        s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}

/// <summary>Export au format .txt (§8.3, §41).</summary>
public sealed class TextExporter : IExporter
{
    public string Id => "txt";
    public string DisplayName => "Texte accessible";

    public async Task<ExportedFile> ExportAsync(ExportRequest request, CancellationToken ct = default)
    {
        var exportableCount = ExportSelection.Publishable(request.Events).Count;
        var content = PlanningTextFormatter.Format(request.Events, request.TextFormat);

        Directory.CreateDirectory(request.OutputFolder);
        var baseName = request.BaseName ?? FileNaming.BuildBaseName(request.Promotion, request.PeriodStart,
            request.PeriodEnd, request.GroupLabel, request.GeneratedAt);
        var path = FileNaming.ResolveNonColliding(request.OutputFolder, baseName, "txt");

        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        await AtomicFile.WriteTextAsync(path, content, utf8NoBom, ct);

        if (!File.Exists(path)) throw new IOException($"Le fichier texte n'a pas été créé : {path}");
        var writtenBack = await File.ReadAllTextAsync(path, ct);
        if (string.IsNullOrWhiteSpace(writtenBack) && exportableCount > 0)
        {
            throw new IOException($"Le fichier texte généré est vide alors que {exportableCount} événement(s) étaient attendus : {path}");
        }

        var info = new FileInfo(path);
        return new ExportedFile(path, info.Length, exportableCount);
    }
}
