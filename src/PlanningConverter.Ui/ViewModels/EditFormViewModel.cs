using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlanningConverter.Domain;

namespace PlanningConverter.Ui.ViewModels;

/// <summary>
/// Formulaire de correction d'une séance (cahier §9.3). Les libellés restent visibles après saisie.
/// « Enregistrer » applique ; « Annuler » abandonne. Une heure invalide est refusée avec un message.
/// </summary>
public sealed partial class EditFormViewModel : ObservableObject
{
    private static readonly string[] TimeFormats = ["HH:mm", "H:mm", "HH'h'mm", "H'h'mm", "HH'h'", "H'h'"];

    private readonly ScheduleEvent _original;
    private readonly Action<ScheduleEvent> _save;
    private readonly Action _cancel;

    [ObservableProperty] private string title;
    [ObservableProperty] private string ue;
    [ObservableProperty] private string startText;
    [ObservableProperty] private string endText;
    [ObservableProperty] private string teacher;
    [ObservableProperty] private string room;
    [ObservableProperty] private string notes;
    [ObservableProperty] private string? errorMessage;

    public string Heading { get; }
    public string SourceText { get; }
    public bool IsAllDay => _original.IsAllDay;

    public EditFormViewModel(ScheduleEvent ev, Action<ScheduleEvent> save, Action cancel)
    {
        _original = ev;
        _save = save;
        _cancel = cancel;

        title = ev.Title ?? "";
        ue = ev.Ue ?? "";
        startText = ev.StartTime?.ToString("HH:mm") ?? "";
        endText = ev.EndTime?.ToString("HH:mm") ?? "";
        teacher = ev.Teacher ?? "";
        room = ev.Room ?? "";
        notes = ev.Notes ?? "";

        var rowVm = new EventRowViewModel(ev);
        Heading = $"Modifier le cours : {rowVm.TitleText}";
        SourceText = ev.OriginalContent;
    }

    private static bool TryParseTime(string text, out TimeOnly? value)
    {
        value = null;
        var cleaned = text.Replace(" ", "").ToLowerInvariant();
        if (cleaned.Length == 0) return true;
        if (TimeOnly.TryParseExact(cleaned, TimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
        {
            value = t;
            return true;
        }
        return false;
    }

    private static string? NullIfBlank(string text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    [RelayCommand]
    private void Save()
    {
        if (!TryParseTime(StartText, out var start))
        {
            ErrorMessage = "L'heure de début n'est pas valide. Exemple : 08:30 ou 8h30.";
            return;
        }
        if (!TryParseTime(EndText, out var end))
        {
            ErrorMessage = "L'heure de fin n'est pas valide. Exemple : 10:30 ou 10h30.";
            return;
        }

        ErrorMessage = null;
        var roomValue = NullIfBlank(Room);
        var updated = _original with
        {
            Title = NullIfBlank(Title),
            Ue = NullIfBlank(Ue),
            StartTime = start,
            EndTime = end,
            Teacher = NullIfBlank(Teacher),
            Room = roomValue,
            Location = roomValue,
            Notes = NullIfBlank(Notes),
            Revision = _original.Revision + 1,
        };
        _save(updated);
    }

    [RelayCommand]
    private void Cancel() => _cancel();
}
