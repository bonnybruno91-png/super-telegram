using PlanningConverter.Domain;

namespace PlanningConverter.Validation;

/// <summary>
/// Deuxième passe après l'analyse structurelle : recalcule, à partir des valeurs EFFECTIVES de
/// chaque séance, les anomalies qui dépendent de ses champs (date, heures, intitulé) et de la
/// comparaison avec les autres séances (doublons, chevauchements), puis le statut.
///
/// Les anomalies « intrinsèques » héritées de l'analyse sont écartées avant recalcul : c'est ce
/// qui permet à une correction (ex. saisie d'une heure de fin) de lever le blocage, et à des
/// validations répétées de ne jamais accumuler les mêmes alertes. Les observations propres à la
/// lecture de la source (jour incohérent, salle/groupe ambigus, journée entière suggérée…) sont
/// conservées telles quelles.
/// </summary>
public static class ValidationEngine
{
    private static readonly HashSet<IssueCode> RecomputedCodes =
    [
        IssueCode.MissingDate, IssueCode.MissingStartTime, IssueCode.MissingEndTime,
        IssueCode.MissingTitle, IssueCode.EndBeforeStart, IssueCode.Overlap, IssueCode.Duplicate,
    ];

    public static AnalysisResult Validate(AnalysisResult raw)
    {
        var baseEvents = raw.Events
            .Select(ev => ev with { Issues = ev.Issues.Where(i => !RecomputedCodes.Contains(i.Code)).ToList() })
            .ToList();

        var duplicates = DuplicateDetector.Detect(baseEvents);
        var overlaps = OverlapDetector.Detect(baseEvents);

        var revised = baseEvents.Select(ev =>
        {
            var issues = ev.Issues.ToList();
            issues.AddRange(IntrinsicIssues(ev));
            if (duplicates.TryGetValue(ev.Id, out var dup)) issues.Add(dup);
            if (overlaps.TryGetValue(ev.Id, out var ovl)) issues.Add(ovl);

            var hasBlocking = issues.Any(i => i.Severity == IssueSeverity.Blocking);
            var confidence = ConfidenceCalculator.Compute(issues);
            var status = ev.Status == EventStatus.Ignored
                ? EventStatus.Ignored
                : ConfidenceCalculator.StatusFor(confidence, hasBlocking);

            return ev with { Issues = issues, Confidence = confidence, Status = status };
        }).ToList();

        return raw with { Events = revised };
    }

    /// <summary>Anomalies bloquantes déduites des champs actuels de la séance.</summary>
    public static IEnumerable<EventIssue> IntrinsicIssues(ScheduleEvent ev)
    {
        if (ev.Date is null)
        {
            yield return new EventIssue(IssueCode.MissingDate, IssueSeverity.Blocking,
                "Aucune date identifiable pour cette ligne.", Field: nameof(ScheduleEvent.Date));
        }

        if (string.IsNullOrWhiteSpace(ev.Title))
        {
            yield return new EventIssue(IssueCode.MissingTitle, IssueSeverity.Blocking,
                "Aucun intitulé de cours identifiable.", Field: nameof(ScheduleEvent.Title));
        }

        if (ev.IsAllDay) yield break;

        if (ev.StartTime is null && ev.EndTime is null)
        {
            yield return new EventIssue(IssueCode.MissingStartTime, IssueSeverity.Blocking,
                "Aucun horaire de début identifiable.", Field: nameof(ScheduleEvent.StartTime));
            yield return new EventIssue(IssueCode.MissingEndTime, IssueSeverity.Blocking,
                "Aucun horaire de fin identifiable.", Field: nameof(ScheduleEvent.EndTime));
        }
        else if (ev.StartTime is null)
        {
            yield return new EventIssue(IssueCode.MissingStartTime, IssueSeverity.Blocking,
                "Heure de début manquante.", Field: nameof(ScheduleEvent.StartTime));
        }
        else if (ev.EndTime is null)
        {
            // Jamais de durée inventée (cahier §4.3) : la fin manquante bloque seulement CETTE séance.
            yield return new EventIssue(IssueCode.MissingEndTime, IssueSeverity.Blocking,
                "Heure de fin manquante.", Field: nameof(ScheduleEvent.EndTime));
        }
        else if (ev.EndTime < ev.StartTime)
        {
            yield return new EventIssue(IssueCode.EndBeforeStart, IssueSeverity.Blocking,
                $"L'heure de fin {ev.EndTime:HH:mm} est antérieure à l'heure de début {ev.StartTime:HH:mm}.",
                Field: nameof(ScheduleEvent.EndTime));
        }
    }
}
