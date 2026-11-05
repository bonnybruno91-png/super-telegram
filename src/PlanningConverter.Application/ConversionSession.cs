using System.Text.Json;
using PlanningConverter.Domain;
using PlanningConverter.Validation;

namespace PlanningConverter.Application;

/// <summary>
/// L'unique objet mutable de tout le pipeline (plan §3.3). Tout le reste — lecture Excel,
/// analyse, validation, export — est composé de fonctions pures sur des types immuables ;
/// c'est cette classe qui porte l'état "en cours d'édition" et l'historique d'annulation.
///
/// Toute modification repasse par <see cref="ValidationEngine"/> : corriger un événement peut
/// faire apparaître ou disparaître un doublon ou un chevauchement avec un autre — recalculer
/// uniquement l'événement modifié serait incorrect.
/// </summary>
public sealed class ConversionSession
{
    private readonly List<AnalysisResult> _undoStack = [];
    private readonly List<AnalysisResult> _redoStack = [];

    public string SessionId { get; }
    public string SourceFilePath { get; }
    public string SourceFileHash { get; }
    public AnalysisResult Current { get; private set; }
    public bool IsDirty { get; private set; }
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public ConversionSession(string sourceFilePath, string sourceFileHash, AnalysisResult initial)
        : this(Guid.NewGuid().ToString(), sourceFilePath, sourceFileHash, initial)
    {
    }

    private ConversionSession(string sessionId, string sourceFilePath, string sourceFileHash, AnalysisResult initial)
    {
        SessionId = sessionId;
        SourceFilePath = sourceFilePath;
        SourceFileHash = sourceFileHash;
        Current = initial;
    }

    /// <summary>
    /// Remplace un événement (édition manuelle, §31) et revalide l'ensemble.
    /// Marque le champ comme modifié par l'utilisateur — il ne sera plus jamais
    /// re-proposé comme "à vérifier" pour la même raison automatique (§32).
    /// </summary>
    public void ReplaceEvent(Guid eventId, ScheduleEvent updated)
    {
        PushUndo();

        var edited = updated with { EditedByUser = true };
        var newEvents = Current.Events.Select(e => e.Id == eventId ? edited : e).ToList();
        var revalidated = ValidationEngine.Validate(Current with { Events = newEvents });

        Current = revalidated;
        IsDirty = true;
        _redoStack.Clear();
    }

    /// <summary>§30 : ignorer un événement sans le supprimer — reste consultable, exclu des exports.</summary>
    public void IgnoreEvent(Guid eventId)
    {
        var target = Current.Events.FirstOrDefault(e => e.Id == eventId);
        if (target is null) return;
        ReplaceEvent(eventId, target with { Status = EventStatus.Ignored });
    }

    /// <summary>§103 : suppression avec possibilité d'annulation (elle passe par la même pile que les éditions).</summary>
    public void DeleteEvent(Guid eventId)
    {
        PushUndo();
        var newEvents = Current.Events.Where(e => e.Id != eventId).ToList();
        Current = ValidationEngine.Validate(Current with { Events = newEvents });
        IsDirty = true;
        _redoStack.Clear();
    }

    public void Undo()
    {
        if (!CanUndo) return;
        _redoStack.Add(Current);
        Current = _undoStack[^1];
        _undoStack.RemoveAt(_undoStack.Count - 1);
        IsDirty = true;
    }

    public void Redo()
    {
        if (!CanRedo) return;
        _undoStack.Add(Current);
        Current = _redoStack[^1];
        _redoStack.RemoveAt(_redoStack.Count - 1);
        IsDirty = true;
    }

    private void PushUndo()
    {
        _undoStack.Add(Current);
        // Les plannings visés (§115 : hebdomadaires, quelques dizaines d'événements) rendent
        // un historique d'annulation illimité négligeable en mémoire ; pas de plafond pour le MVP.
    }

    public void MarkSaved() => IsDirty = false;

    /// <summary>Sérialisation pour la sauvegarde automatique (§105).</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new ReadOnlySetJsonConverterFactory() },
    };

    public string ToAutosaveJson() => JsonSerializer.Serialize(new AutosaveSnapshot(
        SessionId, SourceFilePath, SourceFileHash, Current, DateTimeOffset.Now), JsonOptions);

    public static ConversionSession? FromAutosaveJson(string json)
    {
        var snapshot = JsonSerializer.Deserialize<AutosaveSnapshot>(json, JsonOptions);
        if (snapshot is null) return null;
        // Même identifiant : la sauvegarde reprise continue d'écraser la même sauvegarde (pas de doublon).
        return new ConversionSession(snapshot.SessionId, snapshot.SourceFilePath, snapshot.SourceFileHash, snapshot.Result);
    }

    private sealed record AutosaveSnapshot(
        string SessionId, string SourceFilePath, string SourceFileHash, AnalysisResult Result, DateTimeOffset SavedAt);
}
