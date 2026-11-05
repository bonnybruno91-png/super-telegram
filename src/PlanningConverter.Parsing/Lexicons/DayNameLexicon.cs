namespace PlanningConverter.Parsing.Lexicons;

/// <summary>
/// Reconnaissance des noms de jours, insensible à la casse et aux formes abrégées.
/// Cahier des charges §16 : LUNDI, Lundi, lundi, Lun., LUN doivent tous être reconnus.
/// </summary>
public static class DayNameLexicon
{
    private static readonly Dictionary<string, DayOfWeek> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["lundi"] = DayOfWeek.Monday, ["lun"] = DayOfWeek.Monday, ["lun."] = DayOfWeek.Monday,
        ["mardi"] = DayOfWeek.Tuesday, ["mar"] = DayOfWeek.Tuesday, ["mar."] = DayOfWeek.Tuesday,
        ["mercredi"] = DayOfWeek.Wednesday, ["mer"] = DayOfWeek.Wednesday, ["mer."] = DayOfWeek.Wednesday,
        ["jeudi"] = DayOfWeek.Thursday, ["jeu"] = DayOfWeek.Thursday, ["jeu."] = DayOfWeek.Thursday,
        ["vendredi"] = DayOfWeek.Friday, ["ven"] = DayOfWeek.Friday, ["ven."] = DayOfWeek.Friday,
        ["samedi"] = DayOfWeek.Saturday, ["sam"] = DayOfWeek.Saturday, ["sam."] = DayOfWeek.Saturday,
        ["dimanche"] = DayOfWeek.Sunday, ["dim"] = DayOfWeek.Sunday, ["dim."] = DayOfWeek.Sunday,
    };

    public static bool TryParse(string text, out DayOfWeek day) => Map.TryGetValue(text.Trim(), out day);
}
