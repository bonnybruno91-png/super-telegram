using PlanningConverter.Domain;

namespace PlanningConverter.Validation;

/// <summary>
/// §27 : score interne, non montré à l'utilisateur, qui pilote uniquement le tri et le statut.
/// Départ à 1.00, pénalités cumulatives. Un problème bloquant écrase tout : confiance à 0,
/// statut Error, sans exception.
/// </summary>
public static class ConfidenceCalculator
{
    private static readonly Dictionary<IssueCode, double> Penalties = new()
    {
        [IssueCode.WeekdayMismatch] = 0.10,
        [IssueCode.LocationAmbiguous] = 0.10,
        [IssueCode.AllDayCandidate] = 0.20,
        [IssueCode.Duplicate] = 0.05,
        [IssueCode.Overlap] = 0.05,
        [IssueCode.InvalidTime] = 0.05,
        [IssueCode.YearInferred] = 0.10,
        [IssueCode.WeekAnchorInferred] = 0.15,
        [IssueCode.UnmappedColumn] = 0.15,
    };

    public static double Compute(IReadOnlyList<EventIssue> issues)
    {
        if (issues.Any(i => i.Severity == IssueSeverity.Blocking))
        {
            return 0.0;
        }

        var confidence = 1.0;
        foreach (var issue in issues)
        {
            if (Penalties.TryGetValue(issue.Code, out var penalty))
            {
                confidence -= penalty;
            }
        }
        return Math.Clamp(confidence, 0.0, 1.0);
    }

    public static EventStatus StatusFor(double confidence, bool hasBlockingIssue)
    {
        if (hasBlockingIssue) return EventStatus.Error;
        return confidence switch
        {
            >= 0.85 => EventStatus.Valid,
            >= 0.50 => EventStatus.Warning,
            _ => EventStatus.Error
        };
    }
}
