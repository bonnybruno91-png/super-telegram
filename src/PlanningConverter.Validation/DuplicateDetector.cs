using PlanningConverter.Domain;

namespace PlanningConverter.Validation;

/// <summary>§53, VER-05. Critères : date, heures, intitulé, groupe, salle et enseignant (deux activités
/// différentes qui partagent un horaire ne sont pas des doublons).</summary>
public static class DuplicateDetector
{
    public static IReadOnlyDictionary<Guid, EventIssue> Detect(IReadOnlyList<ScheduleEvent> events)
    {
        var result = new Dictionary<Guid, EventIssue>();

        var groups = events
            .Where(e => e.Status != EventStatus.Ignored)
            .GroupBy(e => (e.Date, e.StartTime, e.EndTime,
                Title: e.Title?.Trim().ToUpperInvariant(),
                Group: e.Group?.Trim().ToUpperInvariant() ?? "",
                Room: (e.Room ?? e.Location)?.Trim().ToUpperInvariant() ?? "",
                Teacher: e.Teacher?.Trim().ToUpperInvariant() ?? ""))
            .Where(g => g.Key.Date is not null && g.Key.Title is not null && g.Count() > 1);

        foreach (var group in groups)
        {
            foreach (var ev in group)
            {
                result[ev.Id] = new EventIssue(
                    IssueCode.Duplicate, IssueSeverity.Warning,
                    $"{group.Count()} événements identiques détectés le {ev.Date:dd/MM/yyyy} à {ev.StartTime:HH:mm} : '{ev.Title}'.",
                    Field: nameof(ScheduleEvent.Title));
            }
        }

        return result;
    }
}
