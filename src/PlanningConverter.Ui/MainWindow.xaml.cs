using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using PlanningConverter.Ui.ViewModels;

namespace PlanningConverter.Ui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldVm) oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        if (e.NewValue is MainViewModel newVm) newVm.PropertyChanged += OnViewModelPropertyChanged;
    }

    /// <summary>
    /// Un changement de texte seul n'est pas toujours annoncé : on lève explicitement
    /// l'événement de région live pour que NVDA/Narrateur lisent le message de statut.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.StatusMessage)) return;
        Dispatcher.BeginInvoke(() =>
        {
            var peer = UIElementAutomationPeer.FromElement(StatusText)
                       ?? UIElementAutomationPeer.CreatePeerForElement(StatusText);
            peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        });
    }
}
