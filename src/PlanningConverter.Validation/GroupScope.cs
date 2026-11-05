using PlanningConverter.Domain;

namespace PlanningConverter.Validation;

/// <summary>
/// Portée de groupe d'une séance, pour comparer doublons et chevauchements (VER-04, VER-05).
/// Pas de mention de groupe = séance commune. Provisoire : le champ <c>Group</c> est encore une
/// chaîne (« P1 IFMKDV, S1 IFMKDV ») ; le lot 1 le remplace par un ensemble confirmé par promotion.
/// </summary>
public static class GroupScope
{
    public static IReadOnlySet<string> Of(ScheduleEvent ev)
    {
        if (string.IsNullOrWhiteSpace(ev.Group)) return new HashSet<string>();
        return ev.Group
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(g => g.ToUpperInvariant())
            .ToHashSet();
    }

    /// <summary>Deux séances concernent-elles au moins un même groupe ? Une séance commune concerne tous les groupes.</summary>
    public static bool Intersects(ScheduleEvent a, ScheduleEvent b)
    {
        var ga = Of(a);
        var gb = Of(b);
        if (ga.Count == 0 || gb.Count == 0) return true;
        return ga.Overlaps(gb);
    }
}
