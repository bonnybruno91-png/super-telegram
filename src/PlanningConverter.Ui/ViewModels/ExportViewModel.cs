using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PlanningConverter.Application;
using PlanningConverter.Domain;

namespace PlanningConverter.Ui.ViewModels;

/// <summary>
/// « Enregistrer le planning » (cahier §9.4) : choix des formats avec leur usage, dossier visible,
/// nombre de fichiers annoncé avant écriture, résultat distinguant succès, partiel et échec.
/// </summary>
public sealed partial class ExportViewModel : ObservableObject
{
    private readonly AppServices _services;
    private readonly ConversionSession _session;
    private readonly Action _backToVerify;
    private readonly Action<string> _announce;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SaveCommand))] private bool icsSelected = true;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SaveCommand))] private bool txtSelected = true;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SaveCommand))] private bool csvSelected;
    [ObservableProperty] private string outputFolder;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SaveCommand))] private bool isWriting;
    [ObservableProperty] private bool hasResult;
    [ObservableProperty] private string resultTitle = "";
    [ObservableProperty] private string resultDetail = "";

    public string PlanText { get; }

    public ExportViewModel(AppServices services, ConversionSession session, Action backToVerify, Action<string> announce)
    {
        _services = services;
        _session = session;
        _backToVerify = backToVerify;
        _announce = announce;

        var saved = services.Settings.Load().LastOutputFolder;
        outputFolder = !string.IsNullOrWhiteSpace(saved)
            ? saved
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Planning Converter");

        var summary = PublishSummary.From(session.Current);
        PlanText = summary.Excluded > 0
            ? $"{summary.Publishable} cours seront exportés. {summary.Excluded} cours à compléter seront exclus."
            : $"{summary.Publishable} cours seront exportés.";
    }

    public int FileCount => (IcsSelected ? 1 : 0) + (TxtSelected ? 1 : 0) + (CsvSelected ? 1 : 0);

    public string FileCountText => FileCount switch
    {
        0 => "Choisissez au moins un format.",
        1 => "1 fichier sera créé pour toute la promotion.",
        var n => $"{n} fichiers seront créés pour toute la promotion.",
    };

    partial void OnIcsSelectedChanged(bool value) => OnPropertyChanged(nameof(FileCountText));
    partial void OnTxtSelectedChanged(bool value) => OnPropertyChanged(nameof(FileCountText));
    partial void OnCsvSelectedChanged(bool value) => OnPropertyChanged(nameof(FileCountText));

    private bool CanSave() => !IsWriting && FileCount > 0;

    [RelayCommand]
    private void ChangeFolder()
    {
        var dialog = new OpenFolderDialog { Title = "Choisir le dossier de destination", InitialDirectory = OutputFolder };
        if (dialog.ShowDialog() == true)
        {
            OutputFolder = dialog.FolderName;
        }
    }

    [RelayCommand]
    private void Back() => _backToVerify();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsWriting = true;
        HasResult = false;
        _announce("Écriture des fichiers en cours.");

        try
        {
            var ids = new List<string>();
            if (IcsSelected) ids.Add("ics");
            if (TxtSelected) ids.Add("txt");
            if (CsvSelected) ids.Add("csv");

            var outcome = await _services.Export.ExecuteAsync(
                _session, ids, OutputFolder, TitleFormat.CourseDashTeacher, CsvProfile.Generic, TextFormat.Detailed);

            var lines = outcome.Files.Select(f => Path.GetFileName(f.Path)).ToList();
            var detail = new List<string>();
            if (lines.Count > 0) detail.Add("Fichiers créés :\n" + string.Join("\n", lines));
            if (outcome.Failures.Count > 0) detail.Add("Fichiers non créés :\n" + string.Join("\n", outcome.Failures));
            if (outcome.Excluded.Count > 0)
            {
                detail.Add("Cours exclus (à compléter) :\n" + string.Join("\n",
                    outcome.Excluded.Select(e => $"{new EventRowViewModel(e).TitleText} — {e.Date:dd/MM/yyyy}")));
            }

            ResultTitle = outcome.Failures.Count == 0
                ? "Planning enregistré"
                : outcome.Files.Count > 0 ? "Enregistrement partiel" : "L'enregistrement a échoué";
            ResultDetail = string.Join("\n\n", detail);
            HasResult = true;

            if (outcome.Files.Count > 0)
            {
                var settings = _services.Settings.Load();
                _services.Settings.Save(settings with { LastOutputFolder = OutputFolder });
            }

            _announce($"{ResultTitle}. {outcome.Files.Count} fichier(s) créé(s).");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            ResultTitle = "L'enregistrement a échoué";
            ResultDetail = ex.Message + "\n\nChoisissez un autre dossier ou réessayez.";
            HasResult = true;
            _announce(ResultTitle);
        }
        finally
        {
            IsWriting = false;
        }
    }

    [RelayCommand]
    private void OpenFolder()
    {
        try
        {
            Directory.CreateDirectory(OutputFolder);
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{OutputFolder}\"") { UseShellExecute = true });
        }
        catch (Exception)
        {
            _announce("Impossible d'ouvrir le dossier. Ouvrez-le depuis l'Explorateur : " + OutputFolder);
        }
    }
}
