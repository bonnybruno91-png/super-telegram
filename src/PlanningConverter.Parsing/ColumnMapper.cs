using PlanningConverter.Domain;
using PlanningConverter.Parsing.Lexicons;
using PlanningConverter.Parsing.Normalization;

namespace PlanningConverter.Parsing;

public sealed record ColumnMapping(
    int? TimeColumn, int? TitleColumn, int? TeacherColumn, int? RoomColumn,
    int? HeaderRow, IReadOnlyList<string> Notes, int? GroupColumn = null)
{
    public bool IsUsable => TitleColumn is not null;
}

/// <summary>
/// Détermine quelle colonne porte quelle information. Deux mécanismes, dans cet ordre :
///
/// 1. Par en-tête (§21) : une ligne contenant "Intitulé" (ou un synonyme du lexique)
///    sert de ligne d'en-tête ; chaque cellule de cette ligne est comparée au lexique.
///
/// 2. Par contenu, pour toute colonne sémantique encore non résolue : la colonne "Heure"
///    en particulier n'a pas toujours d'en-tête dans les fichiers réels observés (le
///    3ᵉ fichier du corpus a A2 vide). On la détecte alors par la proportion de cellules
///    dont la première ligne ressemble à un horaire.
///
/// En dernier recours, si au plus 4 colonnes existent et qu'il ne reste rien de mieux,
/// on retombe sur la convention Heure/Intitulé/Intervenant/Salle observée dans les trois
/// fichiers du corpus — mais avec une note de confiance réduite (§27, UnmappedColumn).
/// </summary>
public static class ColumnMapper
{
    private const double ContentDetectionThreshold = 0.5;

    public static ColumnMapping Map(CellGrid grid)
    {
        var notes = new List<string>();
        int? headerRow = null;
        int? timeCol = null, titleCol = null, teacherCol = null, roomCol = null, groupCol = null;

        // 1. Recherche d'une ligne d'en-tête dans les 5 premières lignes.
        for (var r = 0; r < Math.Min(grid.RowCount, 5); r++)
        {
            var matched = new Dictionary<SemanticColumn, int>();
            for (var c = 0; c < grid.ColumnCount; c++)
            {
                var cell = grid.Cell(r, c);
                if (cell is null || cell.IsEmpty) continue;
                var semantic = HeaderSynonymLexicon.Match(cell.RawText);
                if (semantic is not null) matched[semantic.Value] = c;
            }

            if (matched.ContainsKey(SemanticColumn.Title))
            {
                headerRow = r;
                matched.TryGetValue(SemanticColumn.Time, out var tc); timeCol = matched.ContainsKey(SemanticColumn.Time) ? tc : null;
                matched.TryGetValue(SemanticColumn.Title, out var tic); titleCol = tic;
                matched.TryGetValue(SemanticColumn.Teacher, out var tec); teacherCol = matched.ContainsKey(SemanticColumn.Teacher) ? tec : null;
                matched.TryGetValue(SemanticColumn.Room, out var rc); roomCol = matched.ContainsKey(SemanticColumn.Room) ? rc : null;
                groupCol = matched.TryGetValue(SemanticColumn.Group, out var gc) ? gc : null;
                notes.Add($"Ligne d'en-tête détectée en ligne {r + 1}.");
                break;
            }
        }

        // 2. Détection par contenu pour la colonne horaire, si toujours inconnue.
        var scanStart = (headerRow ?? -1) + 1;
        if (timeCol is null)
        {
            var best = FindBestTimeColumn(grid, scanStart);
            if (best is not null)
            {
                timeCol = best;
                notes.Add($"Colonne horaire détectée par contenu (colonne {best + 1}), aucun en-tête reconnu.");
            }
        }

        // 3. Repli sur la convention confirmée par le corpus réel : Heure, Intitulé, Intervenant, Salle.
        if ((titleCol is null || teacherCol is null || roomCol is null || timeCol is null) && grid.ColumnCount <= 4)
        {
            timeCol ??= 0;
            titleCol ??= 1;
            teacherCol ??= 2;
            roomCol ??= 3;
            notes.Add("Repli sur la convention Heure/Intitulé/Intervenant/Salle (colonnes A à D) — confiance réduite.");
        }

        return new ColumnMapping(timeCol, titleCol, teacherCol, roomCol, headerRow, notes, groupCol);
    }

    private static int? FindBestTimeColumn(CellGrid grid, int scanStart)
    {
        int? bestCol = null;
        var bestRatio = 0.0;

        for (var c = 0; c < grid.ColumnCount; c++)
        {
            var total = 0;
            var matches = 0;
            for (var r = scanStart; r < grid.RowCount; r++)
            {
                var cell = grid.Cell(r, c);
                if (cell is null || cell.IsEmpty) continue;
                total++;
                var firstLine = cell.Lines.Count > 0 ? cell.Lines[0] : cell.RawText;
                if (TimeParser.Parse(firstLine).Success) matches++;
            }
            if (total == 0) continue;
            var ratio = (double)matches / total;
            if (ratio > bestRatio)
            {
                bestRatio = ratio;
                bestCol = c;
            }
        }

        return bestRatio >= ContentDetectionThreshold ? bestCol : null;
    }
}
