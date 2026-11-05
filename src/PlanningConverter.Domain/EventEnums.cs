namespace PlanningConverter.Domain;

/// <summary>Statut d'un événement. Cahier des charges §26.</summary>
public enum EventStatus
{
    Valid,
    Warning,
    Error,
    Ignored
}

/// <summary>Nature de l'événement. §23, §24.</summary>
public enum EventKind
{
    Course,
    Exam,
    Defense,      // soutenance
    Meeting,
    Internship,   // stage
    PersonalWork, // "TEMPS PERSONNEL" — observé dans les fichiers réels, bloque un créneau
    Closure,      // fermeture, jour férié
    Vacation,
    Break,        // pause déjeuner — ignorée par défaut, §23
    Other
}

public enum IssueSeverity
{
    Info,
    Warning,
    Blocking
}

/// <summary>
/// Codes d'anomalie. Le regroupement en trois familles reflète le plan §4 :
/// bloquantes (empêchent l'export), avertissements (n'empêchent pas mais méritent l'attention),
/// informations (traçabilité, jamais montrées comme des erreurs).
/// </summary>
public enum IssueCode
{
    // Bloquantes — §56
    MissingDate,
    MissingStartTime,
    MissingEndTime,
    MissingTitle,
    EndBeforeStart,      // §55
    InvalidDate,
    InvalidTime,

    // Avertissements
    YearInferred,        // §17
    WeekAnchorInferred,  // §17
    DateFromDayName,
    WeekdayMismatch,     // le nom du jour et la date ne correspondent pas dans le calendrier grégorien
    Overlap,             // §54
    Duplicate,           // §53
    LowConfidence,
    AllDayCandidate,     // §57 — aucune heure, ressemble à un jour férié/événement pleine journée

    // Informations — §22
    UnmappedColumn,
    BreakDetected,
    RowIgnored,
    ValuePropagated,     // §14, §15 — valeur héritée d'un contexte, pas lue directement
    LocationAmbiguous    // la colonne "Salle" mélange un lieu et un code de groupe
}

public sealed record EventIssue(
    IssueCode Code,
    IssueSeverity Severity,
    string Message,
    string? Field = null,
    string? SuggestedValue = null);
