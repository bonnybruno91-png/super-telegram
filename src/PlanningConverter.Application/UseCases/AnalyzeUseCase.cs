using PlanningConverter.Domain;
using PlanningConverter.Parsing;
using PlanningConverter.Spreadsheet;
using PlanningConverter.Validation;

namespace PlanningConverter.Application.UseCases;

public sealed record AnalysisProgress(string Stage, double Fraction);

/// <summary>
/// Exécute le pipeline d'analyse sur une feuille déjà choisie et produit une
/// <see cref="ConversionSession"/> prête à être éditée. Fonctionne en tâche d'arrière-plan
/// (§115 : l'interface ne doit jamais être bloquée) et rapporte une progression réelle par
/// étape plutôt qu'un pourcentage factice (§116) — les étapes reportées correspondent à de
/// vraies transitions du pipeline, pas à une simulation de temps écoulé.
/// </summary>
public sealed class AnalyzeUseCase(ISpreadsheetReaderFactory readerFactory)
{
    public async Task<ConversionSession> ExecuteAsync(
        string filePath, string fileHash, string sheetName,
        IProgress<AnalysisProgress>? progress = null, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            progress?.Report(new AnalysisProgress("Lecture du classeur", 0.1));
            using var reader = readerFactory.Open(filePath);
            var grid = reader.ReadSheet(sheetName);

            ct.ThrowIfCancellationRequested();
            progress?.Report(new AnalysisProgress("Reconnaissance de la structure", 0.4));
            var raw = AnalysisPipeline.AnalyzeSheet(grid, filePath);

            ct.ThrowIfCancellationRequested();
            progress?.Report(new AnalysisProgress("Vérification des données", 0.8));
            var validated = ValidationEngine.Validate(raw);

            progress?.Report(new AnalysisProgress("Terminé", 1.0));
            return new ConversionSession(filePath, fileHash, validated);
        }, ct);
    }
}
