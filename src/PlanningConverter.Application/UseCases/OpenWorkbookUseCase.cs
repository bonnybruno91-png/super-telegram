using PlanningConverter.Parsing;
using PlanningConverter.Spreadsheet;
using PlanningConverter.Storage;

namespace PlanningConverter.Application.UseCases;

public sealed record OpenWorkbookResult(
    string FilePath,
    string FileHash,
    IReadOnlyList<SheetScore> SheetScores,
    HistoryEntry? PriorConversion);

/// <summary>
/// Ouvre un classeur, calcule son empreinte et note chaque feuille (§12). Ne sélectionne
/// jamais automatiquement une feuille ambiguë : c'est à l'appelant (couche Ui) de trancher
/// quand plusieurs feuilles sont candidates, ou de proposer directement l'unique candidate.
/// </summary>
public sealed class OpenWorkbookUseCase(ISpreadsheetReaderFactory readerFactory, IHistoryStore historyStore)
{
    public OpenWorkbookResult Execute(string filePath)
    {
        var hash = FileHasher.ComputeSha256(filePath);
        var prior = historyStore.FindByFileHash(hash); // §100 : reconversion du même fichier

        using var reader = readerFactory.Open(filePath);
        var scores = reader.ListSheets()
            .Select(info => SheetTriage.Score(reader.ReadSheet(info.Name)))
            .OrderByDescending(s => s.Score)
            .ToList();

        return new OpenWorkbookResult(filePath, hash, scores, prior);
    }
}
