using PlanningConverter.Domain;

namespace PlanningConverter.Parsing;

/// <summary>
/// Point d'entrée du moteur d'analyse pour une feuille déjà sélectionnée. La sélection
/// de la feuille elle-même (quand il y en a plusieurs candidates, §12) reste une décision
/// utilisateur orchestrée par la future couche Application — ce pipeline ne fait que
/// transformer une <see cref="CellGrid"/> en résultat structuré.
/// </summary>
public static class AnalysisPipeline
{
    public static AnalysisResult AnalyzeSheet(CellGrid grid, string sourceFile)
    {
        var trace = new List<TraceEntry> { TraceEntry.Now("AnalysisPipeline", $"Analyse de '{grid.SheetName}' démarrée.") };

        var (events, parseTrace, promotion) = DaySectionParser.Parse(grid, sourceFile);
        trace.AddRange(parseTrace);

        var datedEvents = events.Where(e => e.Date is not null).ToList();
        var periodStart = datedEvents.Count > 0 ? datedEvents.Min(e => e.Date) : null;
        var periodEnd = datedEvents.Count > 0 ? datedEvents.Max(e => e.Date) : null;

        trace.Add(TraceEntry.Now("AnalysisPipeline",
            $"Terminé : {events.Count} événement(s), période {periodStart:yyyy-MM-dd} → {periodEnd:yyyy-MM-dd}."));

        return new AnalysisResult
        {
            SourceFile = sourceFile,
            SheetName = grid.SheetName,
            PlanningName = promotion,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Events = events,
            Trace = trace,
        };
    }
}
