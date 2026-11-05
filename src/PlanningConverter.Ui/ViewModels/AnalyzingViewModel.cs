using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PlanningConverter.Ui.ViewModels;

/// <summary>Analyse en cours : étape réelle du pipeline (jamais un pourcentage inventé) et annulation.</summary>
public sealed partial class AnalyzingViewModel : ObservableObject
{
    private readonly Action _cancel;

    [ObservableProperty]
    private string stage = "Préparation";

    public string FileName { get; }

    public AnalyzingViewModel(string fileName, Action cancel)
    {
        FileName = fileName;
        _cancel = cancel;
    }

    [RelayCommand]
    private void Cancel() => _cancel();
}
