using PlanningConverter.Application.UseCases;
using PlanningConverter.Export;
using PlanningConverter.Spreadsheet;
using PlanningConverter.Storage;

namespace PlanningConverter.Application;

/// <summary>
/// Point unique où les implémentations concrètes (NPOI, Ical.Net, CsvHelper, SQLite...) sont
/// assemblées. La couche Ui n'importe jamais <c>PlanningConverter.Spreadsheet</c> ni
/// <c>PlanningConverter.Export</c> — elle appelle <see cref="Build"/> et ne manipule ensuite
/// que des cas d'usage et des interfaces de <c>Application</c>/<c>Storage</c>/<c>Domain</c>.
/// C'est ce qui permet à la règle d'architecture du plan §3.2 ("Ui ne référence jamais
/// Spreadsheet/Parsing/Export") de tenir malgré le besoin bien réel de tout instancier quelque part.
/// </summary>
public sealed record AppServices(
    StoragePaths Paths,
    ISettingsStore Settings,
    IHistoryStore History,
    ITemplateStore Templates,
    IAutosaveStore Autosave,
    OpenWorkbookUseCase OpenWorkbook,
    AnalyzeUseCase Analyze,
    ExportUseCase Export);

public static class CompositionRoot
{
    /// <param name="portableRoot">Non nul pour la version portable (§119) ; null pour l'installation standard.</param>
    public static AppServices Build(string? portableRoot = null)
    {
        var paths = new StoragePaths(portableRoot);

        var settings = new JsonSettingsStore(paths);
        var history = new SqliteHistoryStore(paths);
        var templates = new JsonTemplateStore(paths);
        var autosave = new FileAutosaveStore(paths);

        var readerFactory = new NpoiSpreadsheetReaderFactory();

        var exporters = new Dictionary<string, IExporter>
        {
            ["ics"] = new IcsExporter(),
            ["csv"] = new CsvExporter(),
            ["txt"] = new TextExporter(),
            ["pdf"] = new PdfExporter(),
        };

        var openWorkbook = new OpenWorkbookUseCase(readerFactory, history);
        var analyze = new AnalyzeUseCase(readerFactory);
        var export = new ExportUseCase(exporters, history);

        return new AppServices(paths, settings, history, templates, autosave, openWorkbook, analyze, export);
    }
}
