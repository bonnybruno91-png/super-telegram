using PlanningConverter.Domain;
using PlanningConverter.Export;
using PlanningConverter.Storage;

namespace PlanningConverter.Application.UseCases;

/// <summary>Résultat d'un export : fichiers écrits, échecs et séances exclues (cours à compléter).</summary>
public sealed record ExportOutcome(
    IReadOnlyList<ExportedFile> Files,
    IReadOnlyList<string> Failures,
    IReadOnlyList<ScheduleEvent> Excluded);

/// <summary>
/// Exécute les exportateurs choisis et journalise la conversion dans l'historique.
/// Une séance incomplète (anomalie bloquante) est EXCLUE de l'export et signalée dans
/// <see cref="ExportOutcome.Excluded"/> ; elle n'empêche jamais l'export des autres séances
/// (cahier VER-03). Toute la série partage le même instant de génération et la même racine de nom.
/// </summary>
public sealed class ExportUseCase(IReadOnlyDictionary<string, IExporter> exportersById, IHistoryStore historyStore)
{
    public async Task<ExportOutcome> ExecuteAsync(
        ConversionSession session, IReadOnlyList<string> formatIds, string outputFolder,
        TitleFormat titleFormat, CsvProfile csvProfile, TextFormat textFormat,
        string? groupLabel = null, CancellationToken ct = default)
    {
        var excluded = ExportSelection.Excluded(session.Current.Events);

        var dated = session.Current.Events.Where(e => e.Date is not null).Select(e => e.Date!.Value).ToList();
        if (dated.Count == 0)
        {
            throw new InvalidOperationException(
                "Aucune date fiable n'a été identifiée : corrigez au moins un cours avant d'exporter.");
        }

        var promotion = session.Current.PlanningName ?? "Planning";
        var periodStart = session.Current.PeriodStart ?? dated.Min();
        var periodEnd = session.Current.PeriodEnd ?? dated.Max();
        var generatedAt = DateTimeOffset.Now;

        Directory.CreateDirectory(outputFolder);
        var extensions = formatIds.Select(f => f).Distinct().ToList();
        var baseName = FileNaming.ResolveSeriesBaseName(
            outputFolder,
            FileNaming.BuildBaseName(promotion, periodStart, periodEnd, groupLabel, generatedAt),
            extensions);

        var request = new ExportRequest
        {
            Events = session.Current.Events,
            OutputFolder = outputFolder,
            Promotion = promotion,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            GroupLabel = groupLabel,
            GeneratedAt = generatedAt,
            BaseName = baseName,
            TitleFormat = titleFormat,
            CsvProfile = csvProfile,
            TextFormat = textFormat,
        };

        var files = new List<ExportedFile>();
        var failures = new List<string>();

        foreach (var id in formatIds)
        {
            if (!exportersById.TryGetValue(id, out var exporter))
            {
                failures.Add($"Format d'export inconnu : '{id}'.");
                continue;
            }

            try
            {
                var result = await exporter.ExportAsync(request, ct);
                files.Add(result);
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
            {
                // §83 : message compréhensible, pas de fuite d'exception technique vers l'appelant UI.
                failures.Add($"{exporter.DisplayName} : {ex.Message}");
            }
        }

        if (files.Count > 0)
        {
            historyStore.Add(new HistoryEntry
            {
                Id = Guid.NewGuid(),
                SourceFileName = Path.GetFileName(session.SourceFilePath),
                SourceFileHash = session.SourceFileHash,
                Promotion = promotion,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                ConvertedAt = DateTimeOffset.Now,
                EventCount = session.Current.TotalCount,
                ValidCount = session.Current.ValidCount,
                WarningCount = session.Current.WarningCount,
                ErrorCount = session.Current.ErrorCount,
                ExportedFormats = formatIds,
            });
        }

        return new ExportOutcome(files, failures, excluded);
    }
}
