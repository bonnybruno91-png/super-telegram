using NPOI.SS.UserModel;
using NPOI.SS.Util;
using PlanningConverter.Domain;

namespace PlanningConverter.Spreadsheet;

/// <summary>
/// Lecture de classeur via NPOI. Couvre .xlsx (XSSF) et .xls (HSSF) de façon transparente :
/// <see cref="WorkbookFactory.Create(Stream)"/> choisit l'implémentation d'après le contenu réel
/// du flux, pas d'après l'extension du fichier.
///
/// Le fichier est copié en mémoire avant ouverture (§84 : ne jamais verrouiller le fichier
/// source, y compris s'il est déjà ouvert dans Excel — la copie évite tout conflit de handle).
/// Une limite de taille protège contre un classeur anormalement volumineux (§131).
/// </summary>
public sealed class NpoiSpreadsheetReaderFactory : ISpreadsheetReaderFactory
{
    private const long MaxFileSizeBytes = 100L * 1024 * 1024; // 100 Mo — au-delà, ce n'est plus un planning hebdomadaire.

    public ISpreadsheetReader Open(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Fichier introuvable.", filePath);
        }

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > MaxFileSizeBytes)
        {
            throw new WorkbookTooLargeException(
                $"Fichier de {fileInfo.Length / 1024 / 1024} Mo, limite {MaxFileSizeBytes / 1024 / 1024} Mo.");
        }

        MemoryStream buffer;
        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            buffer = new MemoryStream();
            fileStream.CopyTo(buffer);
            buffer.Position = 0;
        }
        catch (IOException ex)
        {
            throw new WorkbookLockedException($"Impossible d'ouvrir '{filePath}' en lecture.", ex);
        }

        var format = SpreadsheetFormatDetector.Detect(buffer);
        if (format == DetectedFormat.Unknown)
        {
            throw new UnsupportedWorkbookFormatException(
                $"Signature binaire non reconnue pour '{filePath}' — ni OOXML ni BIFF.");
        }

        IWorkbook workbook;
        try
        {
            workbook = WorkbookFactory.Create(buffer);
        }
        catch (NPOI.OpenXml4Net.Exceptions.InvalidFormatException ex)
        {
            throw new CorruptWorkbookException($"Archive OOXML invalide pour '{filePath}'.", ex);
        }
        catch (Exception ex) when (ex.GetType().Name.Contains("EncryptedDocumentException"))
        {
            throw new ProtectedWorkbookException($"Classeur chiffré : '{filePath}'.", ex);
        }
        catch (Exception ex)
        {
            // Filet de sécurité générique : §145, aucune erreur de fichier Excel ne doit
            // provoquer la fermeture brutale de l'application. On requalifie en erreur
            // métier connue plutôt que de laisser fuiter une exception NPOI brute.
            throw new CorruptWorkbookException($"Échec d'ouverture de '{filePath}'.", ex);
        }

        return new NpoiSpreadsheetReader(workbook, filePath);
    }
}

public sealed class NpoiSpreadsheetReader(IWorkbook workbook, string sourceFilePath) : ISpreadsheetReader
{
    private readonly IWorkbook _workbook = workbook;
    private readonly string _sourceFilePath = sourceFilePath;
    private bool _disposed;

    public IReadOnlyList<SheetInfo> ListSheets()
    {
        var result = new List<SheetInfo>();
        for (var i = 0; i < _workbook.NumberOfSheets; i++)
        {
            var sheet = _workbook.GetSheetAt(i);
            var nonEmpty = 0;
            var maxCol = 0;
            for (var r = sheet.FirstRowNum; r <= sheet.LastRowNum; r++)
            {
                var row = sheet.GetRow(r);
                if (row is null) continue;
                for (var c = row.FirstCellNum; c < row.LastCellNum; c++)
                {
                    if (c < 0) continue;
                    var cell = row.GetCell(c);
                    if (cell is not null && cell.CellType != CellType.Blank)
                    {
                        nonEmpty++;
                        maxCol = Math.Max(maxCol, c + 1);
                    }
                }
            }
            result.Add(new SheetInfo(sheet.SheetName, sheet.LastRowNum + 1, maxCol, nonEmpty));
        }
        return result;
    }

    public CellGrid ReadSheet(string sheetName)
    {
        var sheet = _workbook.GetSheet(sheetName)
            ?? throw new ArgumentException($"Feuille '{sheetName}' introuvable.", nameof(sheetName));

        var rowCount = sheet.LastRowNum + 1;
        var columnCount = 0;
        for (var r = 0; r < rowCount; r++)
        {
            var row = sheet.GetRow(r);
            if (row is not null) columnCount = Math.Max(columnCount, row.LastCellNum);
        }
        columnCount = Math.Max(columnCount, 1);

        var merges = new List<MergedRegion>();
        for (var i = 0; i < sheet.NumMergedRegions; i++)
        {
            var region = sheet.GetMergedRegion(i);
            merges.Add(new MergedRegion(region.FirstRow, region.LastRow, region.FirstColumn, region.LastColumn));
        }

        var grid = new CellGrid(sheetName, rowCount, columnCount, merges);
        var evaluator = _workbook.GetCreationHelper().CreateFormulaEvaluator();

        for (var r = 0; r < rowCount; r++)
        {
            var row = sheet.GetRow(r);
            for (var c = 0; c < columnCount; c++)
            {
                var cell = row?.GetCell(c);
                var sourceCell = ReadCell(cell, sheetName, r, c, merges, evaluator);
                grid.Set(r, c, sourceCell);
            }
        }

        return grid;
    }

    private static SourceCell ReadCell(
        ICell? cell, string sheetName, int row, int column,
        List<MergedRegion> merges, IFormulaEvaluator evaluator)
    {
        var a1 = new CellReference(row, column).FormatAsString();
        var region = merges.FirstOrDefault(m => m.Contains(row, column));
        var isAnchor = region is not null && region.FirstRow == row && region.FirstColumn == column;

        if (cell is null || cell.CellType == CellType.Blank)
        {
            return new SourceCell
            {
                SheetName = sheetName, Row = row, Column = column, A1Reference = a1,
                Kind = CellValueKind.Empty, RawText = "", Merge = region, IsMergedAnchor = isAnchor
            };
        }

        var effectiveType = cell.CellType == CellType.Formula ? cell.CachedFormulaResultType : cell.CellType;

        try
        {
            switch (effectiveType)
            {
                case CellType.String:
                    var text = (cell.CellType == CellType.Formula ? cell.StringCellValue : cell.StringCellValue) ?? "";
                    return new SourceCell
                    {
                        SheetName = sheetName, Row = row, Column = column, A1Reference = a1,
                        Kind = cell.CellType == CellType.Formula ? CellValueKind.Formula : CellValueKind.Text,
                        RawText = text, TypedValue = text, Merge = region, IsMergedAnchor = isAnchor
                    };

                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        var dt = cell.DateCellValue;
                        // Heure seule (Excel : date de base 1899/1900) → "HH:mm", lisible par TimeRangeParser.
                        // Date seule → "lundi 01 juin 2026", lisible par DateParser.TryParseDayHeader.
                        // Date + heure → "yyyy-MM-dd HH:mm".
                        var isTimeOnly = dt.HasValue && dt.Value.Year <= 1900;
                        var isDateOnly = dt.HasValue && !isTimeOnly && dt.Value.TimeOfDay == TimeSpan.Zero;
                        var rendered = dt is null ? ""
                            : isTimeOnly ? dt.Value.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture)
                            : isDateOnly ? dt.Value.ToString("dddd dd MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("fr-FR"))
                            : dt.Value.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                        return new SourceCell
                        {
                            SheetName = sheetName, Row = row, Column = column, A1Reference = a1,
                            Kind = isDateOnly ? CellValueKind.Date : CellValueKind.Time,
                            RawText = rendered,
                            TypedValue = dt,
                            Merge = region, IsMergedAnchor = isAnchor
                        };
                    }
                    var num = cell.NumericCellValue;
                    return new SourceCell
                    {
                        SheetName = sheetName, Row = row, Column = column, A1Reference = a1,
                        Kind = CellValueKind.Number,
                        RawText = num.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        TypedValue = num, Merge = region, IsMergedAnchor = isAnchor
                    };

                case CellType.Boolean:
                    var b = cell.BooleanCellValue;
                    return new SourceCell
                    {
                        SheetName = sheetName, Row = row, Column = column, A1Reference = a1,
                        Kind = CellValueKind.Text, RawText = b ? "VRAI" : "FAUX",
                        TypedValue = b, Merge = region, IsMergedAnchor = isAnchor
                    };

                default:
                    return new SourceCell
                    {
                        SheetName = sheetName, Row = row, Column = column, A1Reference = a1,
                        Kind = CellValueKind.Error, RawText = "#ERREUR",
                        Merge = region, IsMergedAnchor = isAnchor
                    };
            }
        }
        catch (Exception)
        {
            // Une cellule illisible ne doit jamais faire tomber toute l'analyse (§145) :
            // elle est simplement marquée en erreur et remonte dans la trace.
            return new SourceCell
            {
                SheetName = sheetName, Row = row, Column = column, A1Reference = a1,
                Kind = CellValueKind.Error, RawText = "#ERREUR",
                Merge = region, IsMergedAnchor = isAnchor
            };
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _workbook.Close();
        _disposed = true;
    }
}
