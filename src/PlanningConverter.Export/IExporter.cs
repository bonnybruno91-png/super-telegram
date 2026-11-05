using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using PlanningConverter.Domain;

namespace PlanningConverter.Export;

public sealed record ExportedFile(string Path, long SizeBytes, int EventCount);

public sealed record ExportRequest
{
    public required IReadOnlyList<ScheduleEvent> Events { get; init; }
    public required string OutputFolder { get; init; }
    public required string Promotion { get; init; }
    public required DateOnly PeriodStart { get; init; }
    public required DateOnly PeriodEnd { get; init; }

    /// <summary>Libellé du groupe destinataire (ex. "Groupe-A") ; null = planning de toute la promotion.</summary>
    public string? GroupLabel { get; init; }

    /// <summary>Instant de génération de la série (nommage + DTSTAMP). Un seul par opération d'export.</summary>
    public DateTimeOffset? GeneratedAt { get; init; }

    /// <summary>Racine de nom déjà résolue pour toute la série (évite les collisions désynchronisées).</summary>
    public string? BaseName { get; init; }

    /// <summary>§38 : format de titre. Défaut recommandé "Cours — Enseignant".</summary>
    public TitleFormat TitleFormat { get; init; } = TitleFormat.CourseDashTeacher;

    /// <summary>§40 : profil CSV choisi.</summary>
    public CsvProfile CsvProfile { get; init; } = CsvProfile.Generic;

    /// <summary>§41 : format texte choisi.</summary>
    public TextFormat TextFormat { get; init; } = TextFormat.Detailed;
}

public interface IExporter
{
    string Id { get; }
    string DisplayName { get; }

    /// <summary>
    /// §146 : ne jamais déclarer un export réussi sans relire le fichier écrit et
    /// vérifier qu'il est non vide et lisible.
    /// </summary>
    Task<ExportedFile> ExportAsync(ExportRequest request, CancellationToken ct = default);
}

/// <summary>Sélection commune à tous les exportateurs : ce qui est publiable, dans l'ordre de diffusion.</summary>
public static class ExportSelection
{
    /// <summary>
    /// Séances non ignorées et sans anomalie bloquante (VER-03 : un cours incomplet est exclu,
    /// pas l'export entier), triées par date puis heure de début, ordre stable en cas d'égalité.
    /// </summary>
    public static IReadOnlyList<ScheduleEvent> Publishable(IEnumerable<ScheduleEvent> events) =>
        events
            .Where(e => e.Status != EventStatus.Ignored && e.Kind != EventKind.Break && !e.HasBlockingIssue)
            .Select((e, index) => (e, index))
            .OrderBy(x => x.e.Date)
            .ThenBy(x => x.e.IsAllDay ? TimeOnly.MinValue : x.e.StartTime)
            .ThenBy(x => x.index)
            .Select(x => x.e)
            .ToList();

    /// <summary>Séances qui seront exclues faute de données indispensables (pour avertir avant l'écriture).</summary>
    public static IReadOnlyList<ScheduleEvent> Excluded(IEnumerable<ScheduleEvent> events) =>
        events.Where(e => e.Status != EventStatus.Ignored && e.Kind != EventKind.Break && e.HasBlockingIssue).ToList();
}

/// <summary>Écriture atomique : fichier temporaire sur le même volume, puis publication (cahier §7).</summary>
public static class AtomicFile
{
    public static async Task WriteTextAsync(string finalPath, string content, Encoding encoding, CancellationToken ct)
    {
        var temp = finalPath + ".part";
        try
        {
            await File.WriteAllTextAsync(temp, content, encoding, ct);
            File.Move(temp, finalPath, overwrite: false);
        }
        finally
        {
            if (File.Exists(temp)) File.Delete(temp);
        }
    }
}

/// <summary>§43 : nommage automatique, sans caractères incompatibles avec Windows.</summary>
public static partial class FileNaming
{
    [GeneratedRegex(@"[<>:""/\\|?*\x00-\x1F]")]
    private static partial Regex InvalidWindowsChars();

    /// <summary>
    /// Avec groupe et/ou date de génération : <c>MK4_Groupe-A_2026-11-09_2026-11-20_export-2026-11-05_1430</c>.
    /// Sans les deux : ancien format <c>Planning_MK4_2026-06-01_2026-06-05</c>.
    /// </summary>
    public static string BuildBaseName(string promotion, DateOnly start, DateOnly end,
        string? groupLabel = null, DateTimeOffset? generatedAt = null)
    {
        var safePromotion = InvalidWindowsChars().Replace(promotion, "_");
        if (groupLabel is null && generatedAt is null)
        {
            return $"Planning_{safePromotion}_{start:yyyy-MM-dd}_{end:yyyy-MM-dd}";
        }

        var parts = new List<string> { safePromotion };
        if (!string.IsNullOrWhiteSpace(groupLabel)) parts.Add(InvalidWindowsChars().Replace(groupLabel.Trim(), "_").Replace(' ', '-'));
        parts.Add($"{start:yyyy-MM-dd}");
        parts.Add($"{end:yyyy-MM-dd}");
        if (generatedAt is { } g) parts.Add($"export-{g.DateTime:yyyy-MM-dd_HHmm}");
        return string.Join('_', parts);
    }

    /// <summary>Ajoute un suffixe " (2)", " (3)"... si le fichier existe déjà — jamais d'écrasement silencieux (§44).</summary>
    public static string ResolveNonColliding(string folder, string baseName, string extension)
    {
        var candidate = Path.Combine(folder, $"{baseName}.{extension}");
        if (!File.Exists(candidate)) return candidate;

        var i = 2;
        while (true)
        {
            candidate = Path.Combine(folder, $"{baseName} ({i}).{extension}");
            if (!File.Exists(candidate)) return candidate;
            i++;
        }
    }

    /// <summary>
    /// Racine de nom commune à toute une série : le même suffixe pour toutes les extensions,
    /// choisi pour qu'aucun fichier de la série n'existe déjà.
    /// </summary>
    public static string ResolveSeriesBaseName(string folder, string baseName, IEnumerable<string> extensions)
    {
        var exts = extensions.ToList();
        var candidate = baseName;
        var i = 2;
        while (exts.Any(e => File.Exists(Path.Combine(folder, $"{candidate}.{e}"))))
        {
            candidate = $"{baseName} ({i})";
            i++;
        }
        return candidate;
    }
}

/// <summary>Construit le titre affiché selon le format choisi (§38).</summary>
public static class TitleBuilder
{
    /// <summary>
    /// Intitulé exact du cours tel que le secrétariat l'écrit : code UE d'origine (ex. « UE18 »)
    /// suivi de l'intitulé. Le code est détaché à l'analyse pour la recherche, jamais perdu ni doublé.
    /// </summary>
    public static string ExactTitle(ScheduleEvent ev)
    {
        var title = ev.Title ?? "";
        return string.IsNullOrWhiteSpace(ev.Ue) ? title : $"{ev.Ue} {title}".Trim();
    }

    public static string Build(ScheduleEvent ev, TitleFormat format)
    {
        var exact = ExactTitle(ev);
        return format switch
        {
            TitleFormat.Simple => exact,
            TitleFormat.CourseDashTeacher => string.IsNullOrWhiteSpace(ev.Teacher) ? exact : $"{exact} — {ev.Teacher}",
            TitleFormat.UeDashCourse => string.IsNullOrWhiteSpace(ev.Ue) ? (ev.Title ?? "") : $"{ev.Ue} — {ev.Title}",
            _ => exact
        };
    }
}

/// <summary>§39 : construit la description en omettant les champs absents (le code UE figure déjà dans le titre).</summary>
public static class DescriptionBuilder
{
    public static string Build(ScheduleEvent ev)
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(ev.Teacher)) lines.Add($"Enseignant : {ev.Teacher}");
        if (!string.IsNullOrWhiteSpace(ev.Room)) lines.Add($"Salle : {ev.Room}");
        if (!string.IsNullOrWhiteSpace(ev.Group)) lines.Add($"Groupe : {ev.Group}");
        if (!string.IsNullOrWhiteSpace(ev.Notes)) lines.Add(ev.Notes);
        return string.Join('\n', lines);
    }
}
