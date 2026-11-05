using PlanningConverter.Domain;

namespace PlanningConverter.Validation;

/// <summary>
/// §54. Avertit seulement — ne supprime ni ne modifie jamais un événement automatiquement.
/// </summary>
public static class OverlapDetector
{
    public static IReadOnlyDictionary<Guid, EventIssue> Detect(IReadOnlyList<ScheduleEvent> events)
    {
        var result = new Dictionary<Guid, EventIssue>();

        var byDate = events
            .Where(e => e.Status != EventStatus.Ignored && !e.IsAllDay && e.Date is not null && e.StartTime is not null && e.EndTime is not null)
            .GroupBy(e => e.Date);

        foreach (var dayGroup in byDate)
        {
            var list = dayGroup.ToList();
            for (var i = 0; i < list.Count; i++)
            {
                for (var j = i + 1; j < list.Count; j++)
                {
                    var a = list[i];
                    var b = list[j];
                    // Consécutives (fin = début) ne se chevauchent pas ; groupes disjoints sont compatibles (VER-04).
                    var overlap = a.StartTime < b.EndTime && b.StartTime < a.EndTime && GroupScope.Intersects(a, b);
                    if (!overlap) continue;

                    if (!result.ContainsKey(a.Id))
                    {
                        result[a.Id] = new EventIssue(IssueCode.Overlap, IssueSeverity.Warning,
                            $"Chevauche '{b.Title}' ({b.StartTime:HH:mm}-{b.EndTime:HH:mm}).",
                            Field: nameof(ScheduleEvent.StartTime));
                    }
                    if (!result.ContainsKey(b.Id))
                    {
                        result[b.Id] = new EventIssue(IssueCode.Overlap, IssueSeverity.Warning,
                            $"Chevauche '{a.Title}' ({a.StartTime:HH:mm}-{a.EndTime:HH:mm}).",
                            Field: nameof(ScheduleEvent.StartTime));
                    }
                }
            }
        }

        return result;
    }
}
