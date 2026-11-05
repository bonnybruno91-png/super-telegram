using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PlanningConverter.Ui.ViewModels;

namespace PlanningConverter.Ui.Views;

public partial class VerifyView : UserControl
{
    public VerifyView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is VerifyViewModel oldVm) oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        if (e.NewValue is VerifyViewModel newVm) newVm.PropertyChanged += OnViewModelPropertyChanged;
    }

    /// <summary>
    /// Ouverture du panneau de correction : le focus clavier arrive sur son titre.
    /// Fermeture : retour au titre de la page, jamais perdu sur un bouton disparu.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(VerifyViewModel.Editing) || sender is not VerifyViewModel vm) return;
        Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
        {
            if (vm.Editing is not null) EditHeading.Focus();
            else PageHeading.Focus();
        }));
    }
}
