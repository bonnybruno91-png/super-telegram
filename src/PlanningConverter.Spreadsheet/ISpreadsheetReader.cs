using PlanningConverter.Domain;

namespace PlanningConverter.Spreadsheet;

public sealed record SheetInfo(string Name, int RowCount, int ColumnCount, int NonEmptyCellCount);

/// <summary>
/// Abstraction de lecture Excel. La couche Parsing ne connaît que cette interface,
/// jamais NPOI directement — c'est ce qui permettra de brancher un jour un lecteur
/// PDF/OCR (§80) sans toucher au moteur d'analyse.
/// </summary>
public interface ISpreadsheetReader : IDisposable
{
    IReadOnlyList<SheetInfo> ListSheets();

    /// <summary>Charge une feuille entière en <see cref="CellGrid"/> normalisée.</summary>
    CellGrid ReadSheet(string sheetName);
}

public interface ISpreadsheetReaderFactory
{
    /// <summary>
    /// Ouvre un classeur en lecture seule à partir d'un chemin. Le fichier n'est
    /// jamais modifié ni verrouillé de façon persistante (§4.5, §84).
    /// </summary>
    ISpreadsheetReader Open(string filePath);
}
