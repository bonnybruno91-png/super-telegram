using System.Text.Json;
using PlanningConverter.Domain;

namespace PlanningConverter.Storage;

/// <summary>
/// Un modèle enregistré associe la structure d'un fichier source (§48) à des règles
/// d'interprétation, pour que le secrétariat n'ait pas à reconfigurer le mapping de colonnes
/// à chaque semaine. Modèle de données posé maintenant ; son utilisation effective par
/// <c>PlanningConverter.Parsing</c> (reconnaissance automatique §49, application des règles)
/// reste à câbler — voir plan de conception, lot L14.
/// </summary>
public sealed record ScheduleTemplate
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }

    /// <summary>Motif approximatif du nom de fichier ou de la promotion, pour la reconnaissance automatique (§49).</summary>
    public string? PromotionPattern { get; init; }

    /// <summary>Position des colonnes, si l'établissement n'utilise pas de synonymes reconnus par défaut.</summary>
    public int? TimeColumn { get; init; }
    public int? TitleColumn { get; init; }
    public int? TeacherColumn { get; init; }
    public int? RoomColumn { get; init; }

    /// <summary>
    /// §20 : seule façon autorisée de combler une heure de fin manquante — jamais par défaut
    /// silencieux, uniquement via une règle explicite que l'utilisateur a lui-même choisie.
    /// </summary>
    public int? DefaultDurationMinutes { get; init; }

    public TitleFormat TitleFormat { get; init; } = TitleFormat.CourseDashTeacher;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;
}

public interface ITemplateStore
{
    IReadOnlyList<ScheduleTemplate> ListAll();
    void Save(ScheduleTemplate template);
    void Delete(Guid id);
}

public sealed class JsonTemplateStore(StoragePaths paths) : ITemplateStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public IReadOnlyList<ScheduleTemplate> ListAll()
    {
        if (!File.Exists(paths.TemplatesFile)) return [];
        try
        {
            var json = File.ReadAllText(paths.TemplatesFile);
            return JsonSerializer.Deserialize<List<ScheduleTemplate>>(json, Options) ?? [];
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return [];
        }
    }

    public void Save(ScheduleTemplate template)
    {
        var all = ListAll().Where(t => t.Id != template.Id).Append(template).ToList();
        WriteAll(all);
    }

    public void Delete(Guid id)
    {
        var all = ListAll().Where(t => t.Id != id).ToList();
        WriteAll(all);
    }

    private void WriteAll(List<ScheduleTemplate> templates)
    {
        var json = JsonSerializer.Serialize(templates, Options);
        var tempFile = paths.TemplatesFile + ".tmp";
        File.WriteAllText(tempFile, json);
        File.Move(tempFile, paths.TemplatesFile, overwrite: true);
    }
}
