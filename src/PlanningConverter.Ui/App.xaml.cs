using System.Windows;
using System.Windows.Threading;
using PlanningConverter.Application;
using PlanningConverter.Ui.ViewModels;
using PlanningConverter.Ui.Views;
using Serilog;

namespace PlanningConverter.Ui;

public partial class App : System.Windows.Application
{
    public AppServices Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Mode portable si un dossier "data" existe déjà à côté de l'exécutable (§119, §120) —
        // sinon installation standard, chemins %APPDATA%/%LOCALAPPDATA%.
        var exeDir = AppContext.BaseDirectory;
        var portableMarker = Path.Combine(exeDir, "data");
        var portableRoot = Directory.Exists(portableMarker) ? exeDir : null;

        Services = CompositionRoot.Build(portableRoot);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            // §82 : jamais de contenu de cellule ni de nom de personne dans les journaux —
            // seuls les messages écrits explicitement dans le code (jamais les données du
            // classeur) atterrissent ici ; voir la discipline de logging documentée dans
            // DEV_NOTES.md avant d'ajouter le moindre Log.Information(donnée_utilisateur).
            .WriteTo.File(
                Path.Combine(Services.Paths.LogsFolder, "planning-converter-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        // §145 : aucune exception non gérée ne doit fermer l'application brutalement.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        var mainViewModel = new MainViewModel(Services);
        var window = new MainWindow { DataContext = mainViewModel };
        window.Show();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Exception non gérée sur le thread d'interface.");
        MessageBox.Show(
            "Une erreur inattendue s'est produite. Les détails ont été enregistrés dans le journal de diagnostic.\n\n" +
            "L'application va continuer, mais si le problème persiste, utilisez le bouton \"Exporter le diagnostic\" " +
            "dans les Paramètres pour le signaler.",
            "Planning Converter", MessageBoxButton.OK, MessageBoxImage.Warning);
        e.Handled = true;
    }

    private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Fatal(ex, "Exception non gérée hors du thread d'interface.");
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Exception non observée dans une tâche d'arrière-plan.");
        e.SetObserved();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
