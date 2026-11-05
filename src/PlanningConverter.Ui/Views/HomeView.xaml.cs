using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using PlanningConverter.Ui.ViewModels;

namespace PlanningConverter.Ui.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is HomeViewModel oldVm) oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        if (e.NewValue is HomeViewModel newVm) newVm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // §10.4 : après sélection, le focus se pose automatiquement sur l'action suivante
        // plutôt que de rester perdu sur un bouton qui vient de disparaître.
        if (e.PropertyName == nameof(HomeViewModel.HasSelectedFile) &&
            sender is HomeViewModel { HasSelectedFile: true })
        {
            Dispatcher.BeginInvoke(() => AnalyzeButton.Focus());
        }
    }

    private void OnDragEnter(object sender, DragEventArgs e) => UpdateDragState(e);
    private void OnDragOver(object sender, DragEventArgs e) => UpdateDragState(e);

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        if (DataContext is HomeViewModel vm) vm.IsDragOver = false;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (DataContext is not HomeViewModel vm) return;
        vm.IsDragOver = false;

        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] { Length: > 0 } files) return;

        vm.AcceptFiles(files);
    }

    private void UpdateDragState(DragEventArgs e)
    {
        if (DataContext is not HomeViewModel vm) return;

        var isFile = e.Data.GetDataPresent(DataFormats.FileDrop);
        e.Effects = isFile ? DragDropEffects.Copy : DragDropEffects.None;
        vm.IsDragOver = isFile;
        e.Handled = true;
    }
}
