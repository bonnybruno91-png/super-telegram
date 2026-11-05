using System.Text.RegularExpressions;

namespace PlanningConverter.Parsing.Normalization;

public sealed record TimeRangeResult(TimeOnly? Start, TimeOnly? End, IReadOnlyList<string> Warnings);

/// <summary>
/// Reconnaît une plage horaire sous DEUX formats confirmés :
///
/// 1) Deux lignes dans une même cellule, chacune un horaire isolé — c'est le format
///    exclusif observé dans les fichiers réels du secrétariat ("10:00\n11:00").
///    Aucun séparateur n'est requis dans ce cas : le retour à la ligne EST le séparateur.
///
/// 2) Une seule ligne avec un séparateur explicite — §19 : "08h30 - 10h30", "8h30 à 10h30",
///    "08H30 / 10H30", "8h30–10h". Conservé pour les établissements dont l'export diffère.
///
/// Ne comble jamais une heure de fin manquante par une durée par défaut (I1, §20).
/// </summary>
public static partial class TimeRangeParser
{
    [GeneratedRegex(@"\s*(-|–|—|/|à|au|jusqu'à|>)\s*", RegexOptions.IgnoreCase)]
    private static partial Regex RangeSeparator();

    public static TimeRangeResult Parse(string rawCellText)
    {
        var lines = rawCellText
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length >= 2)
        {
            return ParseMultiLine(lines);
        }

        if (lines.Length == 1)
        {
            return ParseSingleLine(lines[0]);
        }

        return new TimeRangeResult(null, null, ["cellule horaire vide"]);
    }

    private static TimeRangeResult ParseMultiLine(string[] lines)
    {
        var warnings = new List<string>();
        if (lines.Length > 2)
        {
            warnings.Add($"{lines.Length} lignes dans la cellule horaire, seules les deux premières sont utilisées");
        }

        var start = TimeParser.Parse(lines[0]);
        var end = TimeParser.Parse(lines[1]);

        if (!start.Success) warnings.Add(start.Error!);
        if (!end.Success) warnings.Add(end.Error!);

        return new TimeRangeResult(start.Value, end.Value, warnings);
    }

    private static TimeRangeResult ParseSingleLine(string line)
    {
        var parts = RangeSeparator().Split(line).Where(p => p.Trim().Length > 0).ToArray();

        if (parts.Length >= 2)
        {
            var start = TimeParser.Parse(parts[0]);
            var end = TimeParser.Parse(parts[1]);
            var warnings = new List<string>();
            if (!start.Success) warnings.Add(start.Error!);
            if (!end.Success) warnings.Add(end.Error!);
            return new TimeRangeResult(start.Value, end.Value, warnings);
        }

        // Une seule valeur : heure de début présente, fin absente (§20).
        var single = TimeParser.Parse(line);
        if (single.Success)
        {
            return new TimeRangeResult(single.Value, null, ["heure de fin absente"]);
        }

        return new TimeRangeResult(null, null, [$"aucun horaire reconnu dans '{line}'"]);
    }
}
