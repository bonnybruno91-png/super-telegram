using PlanningConverter.Domain;
using PlanningConverter.Export;

namespace PlanningConverter.Application;

/// <summary>
/// Décomptes honnêtes d'une session (cahier VER-01), calculés avec les mêmes règles que l'export :
/// <see cref="Publishable"/> séances partiront dans les fichiers, <see cref="Excluded"/> séances
/// incomplètes en seront exclues, <see cref="NeedsAttention"/> restent à examiner.
/// </summary>
public sealed record PublishSummary(int Total, int Publishable, int Excluded, int NeedsAttention, int Ignored)
{
    public static PublishSummary From(AnalysisResult result)
    {
        var publishable = ExportSelection.Publishable(result.Events).Count;
        var excluded = ExportSelection.Excluded(result.Events).Count;
        return new PublishSummary(
            Total: result.TotalCount,
            Publishable: publishable,
            Excluded: excluded,
            NeedsAttention: result.WarningCount + result.ErrorCount,
            Ignored: result.IgnoredCount);
    }
}
