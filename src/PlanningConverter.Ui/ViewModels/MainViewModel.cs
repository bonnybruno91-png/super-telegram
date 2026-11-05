using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlanningConverter.Application;
using PlanningConverter.Application.UseCases;
using PlanningConverter.Spreadsheet;

namespace PlanningConverter.Ui.ViewModels;

/// <summary>
/// Coquille de navigation : trois étapes (Importer → Vérifier → Exporter), une vue à la fois.
/// Chaque changement d'étape met à jour la région live pour les lecteurs d'écran.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly AppServices _services;
    private CancellationTokenSource? _analysisCts;
    private ConversionSession? _session;
    private VerifyViewModel? _verify;

    [ObservableProperty] private object? currentView;
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private bool isStepImport = true;
    [ObservableProperty] private bool isStepVerify;
    [ObservableProperty] private bool isStepExport;

    public MainViewModel(AppServices services)
    {
        _services = services;
        ShowHome();
    }

    [RelayCommand]
    private void NewConversion()
    {
        _analysisCts?.Cancel();
        _session = null;
        _verify = null;
        ShowHome();
        Announce("Nouvelle conversion. Choisissez un fichier Excel.");
    }

    private void SetStep(bool import, bool verify, bool export)
    {
        IsStepImport = import;
        IsStepVerify = verify;
        IsStepExport = export;
    }

    private void ShowHome(string? error = null)
    {
        SetStep(true, false, false);
        var home = new HomeViewModel(_services, path => _ = StartAnalysisAsync(path));
        if (error is not null) home.ErrorMessage = error;
        CurrentView = home;
    }

    private void Announce(string message) => StatusMessage = message;

    private async Task StartAnalysisAsync(string path)
    {
        _analysisCts?.Cancel();
        var cts = _analysisCts = new CancellationTokenSource();

        var analyzing = new AnalyzingViewModel(Path.GetFileName(path), () => cts.Cancel());
        SetStep(true, false, false);
        CurrentView = analyzing;
        Announce($"Analyse de {analyzing.FileName} en cours.");

        var progress = new Progress<AnalysisProgress>(p => analyzing.Stage = p.Stage);

        try
        {
            var opened = await Task.Run(() => _services.OpenWorkbook.Execute(path), cts.Token);
            var sheet = opened.SheetScores.FirstOrDefault(s => s.IsCandidate);
            if (sheet is null)
            {
                ShowHome("Aucune feuille de ce classeur ne ressemble à un planning. Vérifiez le fichier choisi.");
                Announce("Analyse impossible : aucune feuille de planning trouvée.");
                return;
            }

            var session = await _services.Analyze.ExecuteAsync(
                path, opened.FileHash, sheet.SheetName, progress, cts.Token);

            _session = session;
            ShowVerify();
            Announce(session.Current.StatusAnnouncement());
        }
        catch (OperationCanceledException)
        {
            ShowHome();
            Announce("Analyse annulée.");
        }
        catch (SpreadsheetReadException ex)
        {
            ShowHome(ex.UserMessage);
            Announce("Analyse impossible. " + ex.UserMessage);
        }
        catch (IOException ex)
        {
            ShowHome("Le fichier n'a pas pu être lu : " + ex.Message);
            Announce("Analyse impossible : fichier illisible.");
        }
    }

    private void ShowVerify()
    {
        if (_session is null) return;
        SetStep(false, true, false);
        _verify ??= new VerifyViewModel(_session, ShowExport, Announce);
        CurrentView = _verify;
    }

    private void ShowExport()
    {
        if (_session is null) return;
        SetStep(false, false, true);
        CurrentView = new ExportViewModel(_services, _session, ShowVerify, Announce);
        Announce("Étape 3 sur 3 : enregistrer le planning.");
    }
}
