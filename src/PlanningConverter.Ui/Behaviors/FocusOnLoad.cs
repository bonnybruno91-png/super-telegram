using System.Windows;

namespace PlanningConverter.Ui.Behaviors;

/// <summary>
/// Cahier §10 : au changement d'étape, le focus clavier arrive sur le titre ou le premier élément
/// utile, jamais perdu sur un bouton disparu. À poser sur l'élément qui doit recevoir le focus.
/// </summary>
public static class FocusOnLoad
{
    public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
        "Enabled", typeof(bool), typeof(FocusOnLoad), new PropertyMetadata(false, OnEnabledChanged));

    public static bool GetEnabled(DependencyObject d) => (bool)d.GetValue(EnabledProperty);
    public static void SetEnabled(DependencyObject d, bool value) => d.SetValue(EnabledProperty, value);

    private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element || e.NewValue is not true) return;
        element.Loaded += (_, _) => element.Dispatcher.BeginInvoke(() => element.Focus());
    }
}
