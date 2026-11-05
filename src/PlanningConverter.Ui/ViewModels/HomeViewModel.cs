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
    private readonly Action<string> _onFileAccepted;

    private static readonly string[] AcceptedExtensions = [".xlsx", ".xls"];

    [ObservableProperty]
    private bool isDragOver;

    [ObservableProperty]
    private string? selectedFilePath;

    [ObservableProperty]
    private string? selectedFileName;

    [ObservableProperty]
    private string? selectedFileSizeText;

    [ObservableProperty]
    private string? errorMessage;

    public bool HasSelectedFile => SelectedFilePath is not null;

    public HomeViewModel(AppServices services, Action<string> onFileAccepted)
    {
        _services = services;
        _onFileAccepted = onFileAccepted;
    }

    public static bool IsAcceptedFile(string path) =>
        AcceptedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);

    [RelayCommand]
    private void SelectFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Sélectionner un planning Excel",
            Filter = "Classeurs Excel (*.xlsx;*.xls)|*.xlsx;*.xls",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog() == true)
        {
            AcceptFile(dialog.FileName);
        }
    }

    /// <summary>Appelé par la vue à la fois pour le dialogue et pour le glisser-déposer — un seul chemin de validation.</summary>
    public void AcceptFile(string path)
    {
        if (!IsAcceptedFile(path))
        {
            ErrorMessage = "Ce fichier n'est pas un classeur Excel. Formats acceptés : XLSX et XLS.";
            return;
        }

        ErrorMessage = null;
        SelectedFilePath = path;
        SelectedFileName = Path.GetFileName(path);

        var sizeBytes = new FileInfo(path).Length;
        SelectedFileSizeText = sizeBytes < 1024 * 1024
            ? $"{sizeBytes / 1024.0:0.#} Ko"
            : $"{sizeBytes / 1024.0 / 1024.0:0.#} Mo";

        OnPropertyChanged(nameof(HasSelectedFile));
        AnalyzeCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(HasSelectedFile))]
    private void Analyze()
    {
        if (SelectedFilePath is not null)
        {
            _onFileAccepted(SelectedFilePath);
        }
    }
}
