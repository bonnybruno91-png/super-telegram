using System.Globalization;
using PlanningConverter.Domain;
using PlanningConverter.Validation;

namespace PlanningConverter.Application;

/// <summary>Un fichier analysé, avec sa date de dernière modification (pour départager deux versions d'une semaine).</summary>
public sealed record AnalyzedFile(string Path, DateTime LastWriteUtc, AnalysisResult Result);

/// <summary>
/// Fusionne les semaines de plusieurs fichiers en un seul planning, dans l'ordre chronologique.
/// - Même semaine présente deux fois : la version la plus récente est conservée ; les cours de
///   l'autre sont ignorés (réintégrables dans « Vérifier »), et une remarque l'explique.
/// - Trou entre deux semaines : remarque nommant la période sans fichier (jamais comblée).
/// - Promotions différentes : remarque, aucun mélange silencieux.
/// </summary>
public static class SessionMerger
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    public static AnalysisResult Merge(IReadOnlyList<AnalyzedFile> files)
    {
        if (files.Count == 0) throw new ArgumentException("Aucun fichier à fusionner.", nameof(files));
        if (files.Count == 1) return files[0].Result;

        var notices = new List<string>();
        var ordered = files
            .OrderBy(f => f.Result.PeriodStart ?? DateOnly.MaxValue)
            .ThenBy(f => f.LastWriteUtc)
            .ToList();

        // Même semaine en double : on garde le fichier le plus récent.
        var ignoredFiles = new HashSet<string>();
        foreach (var group in ordered.Where(f => f.Result.PeriodStart is not null).GroupBy(f => f.Result.PeriodStart))
        {
            if (group.Count() < 2) continue;
            var keep = group.OrderByDescending(f => f.LastWriteUtc).First();
            foreach (var other in group.Where(f => f != keep))
            {
                ignoredFiles.Add(other.Path);
                notices.Add($"La semaine du {Fmt(group.Key!.Value)} est présente dans deux fichiers. " +
                            $"« {System.IO.Path.GetFileName(keep.Path)} » (le plus récent) est conservé ; " +
                            $"les cours de « {System.IO.Path.GetFileName(other.Path)} » sont ignorés.");
            }
        }

        // Semaines manquantes entre deux fichiers consécutifs.
        var dated = ordered.Where(f => f.Result.PeriodStart is not null && f.Result.PeriodEnd is not null)
            .GroupBy(f => f.Result.PeriodStart).Select(g => g.First()).ToList();
        for (var i = 1; i < dated.Count; i++)
        {
            var previousEnd = dated[i - 1].Result.PeriodEnd!.Value;
            var nextStart = dated[i].Result.PeriodStart!.Value;
            if (nextStart.DayNumber - previousEnd.DayNumber > 3)
            {
                var from = previousEnd.AddDays(1);
                var to = nextStart.AddDays(-1);
                notices.Add($"Aucun fichier pour la période du {Fmt(from)} au {Fmt(to)}.");
            }
        }

        // Promotions différentes.
        var promos = files.Select(f => f.Result.PlanningName).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
        if (promos.Count > 1)
        {
            notices.Add("Les fichiers semblent concerner plusieurs promotions (" + string.Join(", ", promos) + "). Vérifiez-les.");
        }

        var events = new List<ScheduleEvent>();
        foreach (var f in ordered)
        {
            foreach (var ev in f.Result.Events)
            {
                events.Add(ignoredFiles.Contains(f.Path) ? ev with { Status = EventStatus.Ignored } : ev);
            }
        }

        var kept = ordered.Where(f => !ignoredFiles.Contains(f.Path)).ToList();
        var merged = new AnalysisResult
        {
            SourceFile = ordered[0].Result.SourceFile,
            SheetName = ordered[0].Result.SheetName,
            PlanningName = promos.Count == 1 ? promos[0] : promos.FirstOrDefault(),
            PeriodStart = kept.Select(f => f.Result.PeriodStart).Where(d => d is not null).Min(),
            PeriodEnd = kept.Select(f => f.Result.PeriodEnd).Where(d => d is not null).Max(),
            Events = events,
            Trace = files.SelectMany(f => f.Result.Trace).ToList(),
            Notices = notices,
        };
        return ValidationEngine.Validate(merged);
    }

    private static string Fmt(DateOnly d) => d.ToString("d MMMM yyyy", Fr);
}
