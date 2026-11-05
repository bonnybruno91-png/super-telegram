namespace PlanningConverter.Domain;

/// <summary>
/// Nature d'une valeur de cellule telle que lue dans le classeur source.
/// Cahier des charges §13.
/// </summary>
public enum CellValueKind
{
    Empty,
    Text,
    Number,
    Date,
    Time,
    Formula,
    Error
}

/// <summary>
/// Région de cellules fusionnées. Conservée même si les fichiers observés à ce jour
/// n'en contiennent aucun (voir DEV_NOTES.md) : d'autres établissements en produisent.
/// </summary>
public sealed record MergedRegion(int FirstRow, int LastRow, int FirstColumn, int LastColumn)
{
    public bool Contains(int row, int column) =>
        row >= FirstRow && row <= LastRow && column >= FirstColumn && column <= LastColumn;
}

/// <summary>
/// Une cellule normalisée, indépendante de la bibliothèque de lecture Excel utilisée.
/// C'est le seul type que la couche Parsing connaît de la source (§94 : conservation de la source).
/// </summary>
public sealed record SourceCell
{
    public required string SheetName { get; init; }

    /// <summary>Ligne, base 0 en interne.</summary>
    public required int Row { get; init; }

    /// <summary>Colonne, base 0 en interne.</summary>
    public required int Column { get; init; }

    /// <summary>Référence A1 affichée à l'utilisateur (§95 : "B17").</summary>
    public required string A1Reference { get; init; }

    public required CellValueKind Kind { get; init; }

    /// <summary>Texte tel qu'affiché dans Excel, retours à la ligne internes conservés.</summary>
    public required string RawText { get; init; }

    public object? TypedValue { get; init; }

    /// <summary>Vrai si cette cellule est l'ancre (haut-gauche) d'une région fusionnée.</summary>
    public bool IsMergedAnchor { get; init; }

    public MergedRegion? Merge { get; init; }

    public bool IsEmpty => Kind == CellValueKind.Empty || string.IsNullOrWhiteSpace(RawText);

    /// <summary>Lignes non vides de <see cref="RawText"/> — motif fréquent : plusieurs valeurs dans une cellule.</summary>
    public IReadOnlyList<string> Lines =>
        RawText.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}

/// <summary>
/// Grille de cellules d'une feuille, avec résolution transparente des fusions :
/// toute cellule appartenant à une région fusionnée renvoie la valeur de son ancre.
/// </summary>
public sealed class CellGrid
{
    private readonly SourceCell?[,] _cells;
    private readonly List<MergedRegion> _merges;

    public string SheetName { get; }
    public int RowCount { get; }
    public int ColumnCount { get; }
    public IReadOnlyList<MergedRegion> Merges => _merges;

    public CellGrid(string sheetName, int rowCount, int columnCount, IEnumerable<MergedRegion> merges)
    {
        SheetName = sheetName;
        RowCount = rowCount;
        ColumnCount = columnCount;
        _cells = new SourceCell?[rowCount, columnCount];
        _merges = merges.ToList();
    }

    public void Set(int row, int column, SourceCell cell)
    {
        _cells[row, column] = cell;
    }

    /// <summary>
    /// Renvoie la cellule à la position donnée, résolue à travers les fusions.
    /// Ne lève jamais d'exception pour une position hors bornes : renvoie null.
    /// </summary>
    public SourceCell? Cell(int row, int column)
    {
        if (row < 0 || row >= RowCount || column < 0 || column >= ColumnCount)
        {
            return null;
        }

        var region = _merges.FirstOrDefault(m => m.Contains(row, column));
        if (region is not null && (region.FirstRow != row || region.FirstColumn != column))
        {
            return _cells[region.FirstRow, region.FirstColumn];
        }

        return _cells[row, column];
    }

    public IEnumerable<(int Row, SourceCell?[] Cells)> Rows()
    {
        for (var r = 0; r < RowCount; r++)
        {
            var rowCells = new SourceCell?[ColumnCount];
            for (var c = 0; c < ColumnCount; c++)
            {
                rowCells[c] = Cell(r, c);
            }
            yield return (r, rowCells);
        }
    }

    public bool IsRowEmpty(int row)
    {
        for (var c = 0; c < ColumnCount; c++)
        {
            var cell = Cell(row, c);
            if (cell is not null && !cell.IsEmpty) return false;
        }
        return true;
    }
}
