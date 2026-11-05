namespace PlanningConverter.Domain;

/// <summary>
/// Un événement de planning. Cahier des charges §25, complété par :
/// - Uid stable (§102) ;
/// - InferredFields, la matérialisation de l'invariant "ne jamais inventer en silence" (§4.4, §81) :
///   tout champ qui n'a pas été lu tel quel dans une cellule y figure.
/// </summary>
public sealed record ScheduleEvent
{
    public required Guid Id { get; init; }
    public required string Uid { get; init; }

    // Provenance — §94
    public required string SourceFile { get; init; }
    public required string SourceSheet { get; init; }
    public required int SourceRow { get; init; }
    public required IReadOnlyList<SourceCell> SourceCells { get; init; }
    public required string OriginalContent { get; init; }

    // Données du planning — §25
    public DateOnly? Date { get; init; }
    public DayOfWeek? Day { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }

    /// <summary>§57 : mode "événement journée entière", seulement si explicitement indiqué ou confirmé.</summary>
    public bool IsAllDay { get; init; }

    public string? Title { get; init; }
    public string? Teacher { get; init; }
    public string? Room { get; init; }
    public string? Location { get; init; }
    public string? Ue { get; init; }
    public string? Group { get; init; }       // §97
    public string? Promotion { get; init; }   // §96
    public string? Notes { get; init; }

    // Qualité — §26, §27, §81
    public EventKind Kind { get; init; } = EventKind.Course;
    public EventStatus Status { get; init; } = EventStatus.Valid;
    public double Confidence { get; init; } = 1.0;
    public required IReadOnlyList<EventIssue> Issues { get; init; }
    public required IReadOnlySet<string> InferredFields { get; init; }
    public bool EditedByUser { get; init; }

    /// <summary>Numéro de révision (SEQUENCE iCalendar) : incrémenté quand le contenu change.</summary>
    public int Revision { get; init; }

    public bool HasBlockingIssue => Issues.Any(i => i.Severity == IssueSeverity.Blocking);

    /// <summary>Nom accessible complet d'un événement — utilisé tel quel par la future vue WPF (§62).</summary>
    public string ToAccessibleName()
    {
        var day = Date is { } d ? d.ToString("dddd d MMMM", System.Globalization.CultureInfo.GetCultureInfo("fr-FR")) : "date à vérifier";
        var time = StartTime is { } s && EndTime is { } e
            ? $"{s:HH\\hmm} à {e:HH\\hmm}"
            : "horaire à vérifier";
        var parts = new List<string> { day, time, Title ?? "intitulé à vérifier" };
        if (!string.IsNullOrWhiteSpace(Teacher)) parts.Add(Teacher);
        if (!string.IsNullOrWhiteSpace(Room)) parts.Add($"salle {Room}");
        parts.Add($"statut : {StatusLabel()}");
        return string.Join(", ", parts) + ".";
    }

    private string StatusLabel() => Status switch
    {
        EventStatus.Valid => "validé",
        EventStatus.Warning => "à vérifier",
        EventStatus.Error => "erreur",
        EventStatus.Ignored => "ignoré",
        _ => "inconnu"
    };
}
