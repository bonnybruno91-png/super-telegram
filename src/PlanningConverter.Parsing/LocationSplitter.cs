using PlanningConverter.Parsing.Lexicons;

namespace PlanningConverter.Parsing;

public sealed record LocationSplitResult(string? Room, string? Group, bool WasAmbiguous);

/// <summary>
/// Découverte sur les fichiers réels (voir DEV_NOTES.md) : la colonne "Salle" contient
/// tantôt un vrai lieu ("AMPHI 3 LAENNEC"), tantôt un code de groupe ("P1 IFMKDV"),
/// parfois les deux sur des lignes séparées d'une même cellule.
///
/// Par prudence (I1), on ne supprime jamais l'information brute : chaque ligne est
/// classée lieu ou groupe par heuristique, et le doute est signalé plutôt que masqué.
/// Un modèle enregistré (§48) pourra un jour remplacer cette heuristique par une règle
/// explicite propre à l'établissement.
/// </summary>
public static class LocationSplitter
{
    public static LocationSplitResult Split(string? rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return new LocationSplitResult(null, null, false);
        }

        var lines = rawContent
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var rooms = new List<string>();
        var groups = new List<string>();

        foreach (var line in lines)
        {
            if (GroupCodeLexicon.LooksLikeGroupCode(line))
            {
                groups.Add(line);
            }
            else
            {
                rooms.Add(line);
            }
        }

        var ambiguous = rooms.Count > 0 && groups.Count > 0;

        return new LocationSplitResult(
            rooms.Count > 0 ? string.Join(", ", rooms) : null,
            groups.Count > 0 ? string.Join(", ", groups) : null,
            ambiguous);
    }
}
