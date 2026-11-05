using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PlanningConverter.Application;

namespace PlanningConverter.Ui.ViewModels;

/// <summary>
/// §10 : réception du fichier. Toute la fenêtre sert de zone de dépôt (plan §10.3) —
/// pas une petite cible en pointillés au centre, qui serait un échec d'utilisabilité pour
/// quiconque a un tremblement ou une faible vision. Le glisser-déposer est géré côté vue
/// (événements WPF non bindables proprement en MVVM pur) ; ce ViewModel n'expose que l'état
/// et les actions, indépendamment de la façon dont le fichier a été choisi.
/// </summary>
public sealed partial class HomeViewModel : ObservableObject
{
    private readonly AppServices _services;
    private readonly Action<IReadOnlyList<string>> _onFilesAccepted;
    private readonly Action? _onResume;

    /// <summary>Texte du bouton « Reprendre » (null : aucune conversion à reprendre).</summary>
    public string? ResumeText { get; }
    public bool CanResume => ResumeText is not null;

    [RelayCommand]
    private void Resume() => _onResume?.Invoke();

    private static readonly string[] AcceptedExtensions = [".xlsx", ".xls"];

    [ObservableProperty]
    private bool isDragOver;

    private List<string> _files = [];

    [ObservableProperty]
    private string? selectedFilePath;

    [ObservableProperty]
    private string? selectedFileList;

    [ObservableProperty]
    private string? selectedFileName;

    [ObservableProperty]
    private string? selectedFileSizeText;

    [ObservableProperty]
    private string? errorMessage;

    public bool HasSelectedFile => SelectedFilePath is not null;

    public HomeViewModel(AppServices services, Action<IReadOnlyList<string>> onFilesAccepted,
        string? resumeText = null, Action? onResume = null)
    {
        ResumeText = resumeText;
        _onResume = onResume;
        _services = services;
        _onFilesAccepted = onFilesAccepted;
    }

    public static bool IsAcceptedFile(string path) =>
        AcceptedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);

    [RelayCommand]
    private void SelectFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Sélectionner un ou plusieurs plannings Excel",
            Multiselect = true,
            Filter = "Classeurs Excel (*.xlsx;*.xls)|*.xlsx;*.xls",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog() == true)
        {
            AcceptFiles(dialog.FileNames);
        }
    }

    /// <summary>Appelé par la vue pour le dialogue et le glisser-déposer — un seul chemin de validation.</summary>
    public void AcceptFile(string path) => AcceptFiles([path]);

    public void AcceptFiles(IEnumerable<string> paths)
    {
        var all = paths.ToList();
        var valid = all.Where(IsAcceptedFile).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var rejected = all.Count - valid.Count;

        if (valid.Count == 0)
        {
            ErrorMessage = "Ce fichier n'est pas un classeur Excel. Formats acceptés : XLSX et XLS.";
            return;
        }

        ErrorMessage = rejected > 0
            ? $"{rejected} fichier{(rejected > 1 ? "s ont" : " a")} été ignoré{(rejected > 1 ? "s" : "")} : ce ne sont pas des classeurs Excel."
            : null;

        _files = valid;
        SelectedFilePath = valid[0];
        SelectedFileName = valid.Count == 1 ? Path.GetFileName(valid[0]) : $"{valid.Count} fichiers sélectionnés";
        SelectedFileList = valid.Count == 1 ? null : string.Join("\n", valid.Select(Path.GetFileName));

        var sizeBytes = valid.Sum(f => new FileInfo(f).Length);
        SelectedFileSizeText = sizeBytes < 1024 * 1024
            ? $"{sizeBytes / 1024.0:0.#} Ko"
            : $"{sizeBytes / 1024.0 / 1024.0:0.#} Mo";

        OnPropertyChanged(nameof(HasSelectedFile));
        AnalyzeCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(HasSelectedFile))]
    private void Analyze()
    {
        if (_files.Count > 0)
        {
            _onFilesAccepted(_files);
        }
    }
}
