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
    private int _fileCount = 1;

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

    [RelayCommand]
    private void ShowHelp()
    {
        SetStep(false, false, false);
        CurrentView = new HelpViewModel();
        Announce("Aide.");
    }

    [RelayCommand]
    private void ShowHistory()
    {
        SetStep(false, false, false);
        CurrentView = new HistoryViewModel(_services.History);
        Announce("Historique des conversions.");
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
        var resumable = TryLoadResumable();
        var home = new HomeViewModel(_services, paths => _ = StartAnalysisAsync(paths),
            resumable is null ? null : DescribeResumable(resumable),
            resumable is null ? null : () => Resume(resumable));
        if (error is not null) home.ErrorMessage = error;
        CurrentView = home;
    }

    private ConversionSession? TryLoadResumable()
    {
        try
        {
            var id = _services.Autosave.MostRecentSessionId();
            var json = id is null ? null : _services.Autosave.TryLoad(id);
            var session = json is null ? null : ConversionSession.FromAutosaveJson(json);
            return session is { Current.Events.Count: > 0 } ? session : null;
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or NotSupportedException or IOException or UnauthorizedAccessException)
        {
            return null; // une sauvegarde illisible ne doit jamais empêcher de démarrer
        }
    }

    private static string DescribeResumable(ConversionSession session)
    {
        var fr = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
        var r = session.Current;
        var when = r.PeriodStart is { } s && r.PeriodEnd is { } e
            ? $"du {s.ToString("d MMMM", fr)} au {e.ToString("d MMMM yyyy", fr)}"
            : Path.GetFileName(session.SourceFilePath);
        return $"Reprendre la dernière conversion ({when}, {r.TotalCount} cours)";
    }

    private void Resume(ConversionSession session)
    {
        _session = session;
        _fileCount = 1;
        _verify = null;
        ShowVerify();
        Announce("Conversion reprise. " + session.Current.StatusAnnouncement());
    }

    private void Persist()
    {
        if (_session is null) return;
        try
        {
            _services.Autosave.Save(_session.SessionId, _session.ToAutosaveJson());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Announce("La sauvegarde automatique a échoué. Vos modifications restent dans l'application.");
        }
    }

    private void Announce(string message) => StatusMessage = message;

    private async Task StartAnalysisAsync(IReadOnlyList<string> paths)
    {
        _analysisCts?.Cancel();
        var cts = _analysisCts = new CancellationTokenSource();

        var label = paths.Count == 1 ? Path.GetFileName(paths[0]) : $"{paths.Count} fichiers";
        var analyzing = new AnalyzingViewModel(label, () => cts.Cancel());
        SetStep(true, false, false);
        CurrentView = analyzing;
        Announce($"Analyse de {label} en cours.");

        try
        {
            var analyzed = new List<AnalyzedFile>();
            var hashes = new List<string>();

            for (var i = 0; i < paths.Count; i++)
            {
                var path = paths[i];
                var name = Path.GetFileName(path);
                var prefix = paths.Count > 1 ? $"Fichier {i + 1} sur {paths.Count} : " : "";
                var progress = new Progress<AnalysisProgress>(p => analyzing.Stage = prefix + p.Stage);

                var opened = await Task.Run(() => _services.OpenWorkbook.Execute(path), cts.Token);
                var sheet = opened.SheetScores.FirstOrDefault(s => s.IsCandidate);
                if (sheet is null)
                {
                    ShowHome($"Aucune feuille de « {name} » ne ressemble à un planning. Vérifiez le fichier choisi.");
                    Announce($"Analyse impossible : aucune feuille de planning dans {name}.");
                    return;
                }

                var session = await _services.Analyze.ExecuteAsync(
                    path, opened.FileHash, sheet.SheetName, progress, cts.Token);
                analyzed.Add(new AnalyzedFile(path, File.GetLastWriteTimeUtc(path), session.Current));
                hashes.Add(opened.FileHash);
            }

            var merged = SessionMerger.Merge(analyzed);
            var combinedHash = hashes.Count == 1
                ? hashes[0]
                : Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(string.Join("+", hashes))));

            // Une seule conversion reprenable : la nouvelle remplace les sauvegardes précédentes.
            foreach (var old in _services.Autosave.ListPendingSessions()) _services.Autosave.Clear(old);
            _session = new ConversionSession(paths[0], combinedHash, merged);
            _fileCount = paths.Count;
            _verify = null;
            Persist();
            ShowVerify();
            var notice = merged.Notices.Count > 0 ? $" {merged.Notices.Count} remarque{(merged.Notices.Count > 1 ? "s" : "")} sur les fichiers." : "";
            Announce(merged.StatusAnnouncement() + notice);
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
            ShowHome("Un fichier n'a pas pu être lu : " + ex.Message);
            Announce("Analyse impossible : fichier illisible.");
        }
    }

    private void ShowVerify()
    {
        if (_session is null) return;
        SetStep(false, true, false);
        _verify ??= new VerifyViewModel(_session, _fileCount, ShowExport, Announce, Persist);
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
