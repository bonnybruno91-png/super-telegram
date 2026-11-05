using PlanningConverter.Domain;
using PlanningConverter.Parsing.Lexicons;
using PlanningConverter.Parsing.Normalization;

namespace PlanningConverter.Parsing;

public sealed record SheetScore(string SheetName, int Score, bool IsCandidate);

/// <summary>
/// §12 : un classeur peut contenir plusieurs feuilles ; il faut identifier lesquelles
/// ressemblent à un planning. Score 0–100 combinant densité, présence de noms de jours
/// et présence de motifs horaires. Seuil de candidature : 40 (plan §5, étage 1).
/// </summary>
public static class SheetTriage
{
    private const int CandidateThreshold = 40;

    public static SheetScore Score(CellGrid grid)
    {
        var totalCells = 0;
        var nonEmpty = 0;
        var dayNameHits = 0;
        var timePatternHits = 0;

        for (var r = 0; r < grid.RowCount; r++)
        {
            for (var c = 0; c < grid.ColumnCount; c++)
            {
                var cell = grid.Cell(r, c);
                totalCells++;
                if (cell is null || cell.IsEmpty) continue;
                nonEmpty++;

                foreach (var line in cell.Lines.DefaultIfEmpty(cell.RawText))
                {
                    if (DateParser.TryParseDayHeader(line) is not null) dayNameHits++;
                    if (TimeParser.Parse(line).Success) timePatternHits++;
                }
            }
        }

        if (totalCells == 0) return new SheetScore(grid.SheetName, 0, false);

        var density = (double)nonEmpty / totalCells;
        var densityScore = Math.Min(40, density * 100);
        var dayScore = Math.Min(30, dayNameHits * 6);
        var timeScore = Math.Min(30, timePatternHits * 3);

        var total = (int)Math.Round(densityScore + dayScore + timeScore);
        return new SheetScore(grid.SheetName, total, total >= CandidateThreshold);
    }
}
