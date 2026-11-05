namespace PlanningConverter.Domain;

/// <summary>
/// Une entrée du journal d'analyse : sert au diagnostic (§128) et à l'écran
/// "Voir les données Excel d'origine" (§95). Jamais de donnée personnelle superflue.
/// </summary>
public sealed record TraceEntry(string Stage, string Message, DateTimeOffset At)
{
    public static TraceEntry Now(string stage, string message) => new(stage, message, DateTimeOffset.Now);
}

/// <summary>Résultat de l'analyse d'une feuille. Alimente l'écran §28.</summary>
public sealed record AnalysisResult
{
    public required string SourceFile { get; init; }
    public required string SheetName { get; init; }
    public string? PlanningName { get; init; }
    public DateOnly? PeriodStart { get; init; }
    public DateOnly? PeriodEnd { get; init; }
    public required IReadOnlyList<ScheduleEvent> Events { get; init; }
    public required IReadOnlyList<TraceEntry> Trace { get; init; }

    public int TotalCount => Events.Count(e => e.Status != EventStatus.Ignored);
    public int ValidCount => Events.Count(e => e.Status == EventStatus.Valid);
    public int WarningCount => Events.Count(e => e.Status == EventStatus.Warning);
    public int ErrorCount => Events.Count(e => e.Status == EventStatus.Error);
    public int IgnoredCount => Events.Count(e => e.Status == EventStatus.Ignored);

    public bool ReadyToExport => Events
        .Where(e => e.Status != EventStatus.Ignored)
        .All(e => !e.HasBlockingIssue);

    /// <summary>Message de statut annoncé aux lecteurs d'écran après analyse. §64.</summary>
    public string StatusAnnouncement() =>
        $"Analyse terminée. {TotalCount} événement{(TotalCount > 1 ? "s" : "")} détecté{(TotalCount > 1 ? "s" : "")}. " +
        (WarningCount + ErrorCount == 0
            ? "Aucun élément à vérifier."
            : $"{WarningCount + ErrorCount} élément{(WarningCount + ErrorCount > 1 ? "s" : "")} à vérifier.");
}
