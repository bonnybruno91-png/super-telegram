using CommunityToolkit.Mvvm.ComponentModel;
using PlanningConverter.Application;

namespace PlanningConverter.Ui.ViewModels;

/// <summary>
/// Coquille de navigation (plan §10.1). Une seule fenêtre, une vue à la fois dans la zone
/// de contenu. Pour cette étape du développement, seul l'écran d'accueil est fonctionnel —
/// les use cases d'analyse (V3/V4) ne sont pas encore branchés sur l'action "Analyser" : voir
/// <see cref="OnFileAccepted"/>, qui se contente d'annoncer la réception du fichier plutôt
/// que de simuler un résultat.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly AppServices _services;

    [ObservableProperty]
    private object? currentView;

    /// <summary>Région live (§64) : lu par les lecteurs d'écran à chaque changement.</summary>
    [ObservableProperty]
    private string statusMessage = "";

    public MainViewModel(AppServices services)
    {
        _services = services;
        ShowHome();
    }

    private void ShowHome()
    {
        CurrentView = new HomeViewModel(_services, OnFileAccepted);
    }

    private void OnFileAccepted(string filePath)
    {
        // Prochain lot : brancher AnalyzeUseCase, afficher la progression réelle (§116),
        // puis naviguer vers l'aperçu (V4). Pour l'instant, pas de simulation — on annonce
        // seulement ce qui s'est vraiment passé, conformément à I1.
        StatusMessage = $"Fichier reçu : {Path.GetFileName(filePath)}. " +
                         "L'analyse automatique n'est pas encore branchée dans cette version.";
    }
}
