using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlanningConverter.Application;
using PlanningConverter.Domain;

namespace PlanningConverter.Ui.ViewModels;

/// <summary>
/// « Vérifier le planning » (cahier §9.3) : séances par jour, correction, ignorer/réintégrer,
/// annuler/rétablir. Les décomptes viennent de <see cref="PublishSummary"/>, avec les mêmes
/// règles que l'export : un cours à compléter est exclu, il n'empêche pas d'exporter les autres.
/// </summary>
public sealed partial class VerifyViewModel : ObservableObject
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    private readonly ConversionSession _session;
    private readonly Action _goToExport;
    private readonly Action<string> _announce;
    private readonly int _fileCount;

    public ObservableCollection<DayGroupViewModel> Days { get; } = [];

    [ObservableProperty] private string summaryText = "";
    [ObservableProperty] private string exportText = "";
    [ObservableProperty] private string attentionText = "";
    [ObservableProperty] private string noticesText = "";
    [ObservableProperty] private bool hasNotices;
    [ObservableProperty] private bool hasAttention;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ChooseFormatsCommand))] private bool canExport;
    [ObservableProperty] private bool isEmpty;
    [ObservableProperty] private EditFormViewModel? editing;

    public string FileName { get; }
    public string ContextText { get; private set; } = "";

    public VerifyViewModel(ConversionSession session, int fileCount, Action goToExport, Action<string> announce)
    {
        _fileCount = fileCount;
        _session = session;
        _goToExport = goToExport;
        _announce = announce;
        FileName = Path.GetFileName(session.SourceFilePath);
        Refresh();
    }

    public bool CanUndo => _session.CanUndo;
    public bool CanRedo => _session.CanRedo;

    private void Refresh()
    {
        var result = _session.Current;
        var summary = PublishSummary.From(result);

        Days.Clear();
        var ordered = result.Events
            .Select((e, i) => (e, i))
            .OrderBy(x => x.e.Date ?? DateOnly.MaxValue)
            .ThenBy(x => x.e.IsAllDay ? TimeOnly.MinValue : x.e.StartTime ?? TimeOnly.MaxValue)
            .ThenBy(x => x.i)
            .Select(x => x.e);

        foreach (var day in ordered.GroupBy(e => e.Date))
        {
            var header = day.Key is { } d
                ? EventRowViewModel.Capitalize(d.ToString("dddd d MMMM yyyy", Fr))
                : "Date à vérifier";
            Days.Add(new DayGroupViewModel(header, day.Select(e => new EventRowViewModel(e)).ToList()));
        }

        var promo = result.PlanningName ?? "Promotion non identifiée";
        NoticesText = string.Join("\n", result.Notices);
        HasNotices = result.Notices.Count > 0;

        ContextText = result.PeriodStart is { } s && result.PeriodEnd is { } e
            ? $"{promo}  •  du {s.ToString("d MMMM", Fr)} au {e.ToString("d MMMM yyyy", Fr)}" + (_fileCount > 1 ? $"  •  {_fileCount} fichiers" : "")
            : promo;
        OnPropertyChanged(nameof(ContextText));

        SummaryText = $"{summary.Total} événement{Plural(summary.Total)} détecté{Plural(summary.Total)}";
        HasAttention = summary.NeedsAttention > 0;
        AttentionText = summary.NeedsAttention switch
        {
            0 => "Aucun élément à vérifier.",
            1 => "1 élément à vérifier.",
            var n => $"{n} éléments à vérifier.",
        };

        ExportText = summary.Excluded > 0
            ? $"{summary.Publishable} cours seront exportés. {summary.Excluded} cours à compléter seront exclus."
            : $"{summary.Publishable} cours seront exportés.";
        CanExport = summary.Publishable > 0;
        IsEmpty = summary.Total == 0;

        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    private static string Plural(int n) => n > 1 ? "s" : "";

    [RelayCommand]
    private void Edit(EventRowViewModel? row)
    {
        if (row is null) return;
        Editing = new EditFormViewModel(row.Event, Apply(row), CloseEditor);
    }

    private Action<ScheduleEvent> Apply(EventRowViewModel row) => updated =>
    {
        _session.ReplaceEvent(row.Id, updated);
        Editing = null;
        Refresh();
        _announce("Cours enregistré. " + AttentionText);
    };

    private void CloseEditor()
    {
        Editing = null;
        _announce("Modification annulée.");
    }

    [RelayCommand]
    private void ToggleIgnore(EventRowViewModel? row)
    {
        if (row is null) return;
        if (row.IsIgnored)
        {
            // Réintégrer : le statut est recalculé par la validation.
            _session.ReplaceEvent(row.Id, row.Event with { Status = EventStatus.Valid });
            _announce("Cours réintégré. " + AttentionText);
        }
        else
        {
            _session.IgnoreEvent(row.Id);
            _announce("Cours ignoré : il ne sera pas exporté.");
        }
        Refresh();
    }

    [RelayCommand]
    private void ShowFirstIssue()
    {
        var first = Days.SelectMany(d => d.Items).FirstOrDefault(r => r.IsIncomplete)
                    ?? Days.SelectMany(d => d.Items).FirstOrDefault(r => !r.IsIgnored && r.Event.Status == EventStatus.Warning);
        if (first is not null) Edit(first);
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        _session.Undo();
        Refresh();
        _announce("Dernière modification annulée.");
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        _session.Redo();
        Refresh();
        _announce("Modification rétablie.");
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void ChooseFormats() => _goToExport();
}
