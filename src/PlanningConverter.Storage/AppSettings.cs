using System.Text.Json;
using System.Text.Json.Serialization;
using PlanningConverter.Domain;

namespace PlanningConverter.Storage;

/// <summary>
/// Réglages persistants de l'application. Versionné (<see cref="SchemaVersion"/>) pour
/// pouvoir migrer sans perdre les préférences de l'utilisateur si le format évolue.
/// </summary>
public sealed record AppSettings
{
    public int SchemaVersion { get; init; } = 1;

    /// <summary>§43-§44 : dossier de destination par défaut, mémorisé entre les sessions.</summary>
    public string? LastOutputFolder { get; init; }

    public TitleFormat TitleFormat { get; init; } = TitleFormat.CourseDashTeacher; // D4
    public CsvProfile CsvProfile { get; init; } = CsvProfile.Generic;
    public TextFormat TextFormat { get; init; } = TextFormat.Detailed;

    /// <summary>§23 : les pauses sont exclues par défaut (D5).</summary>
    public bool IncludeBreaks { get; init; } = false;

    /// <summary>§74 : désactivation des animations (réduction des mouvements).</summary>
    public bool AnimationsEnabled { get; init; } = true;

    /// <summary>§69 : thème sombre — posé dès la v1, activable plus tard sans refonte.</summary>
    public bool DarkModeEnabled { get; init; } = true;
}

public interface ISettingsStore
{
    AppSettings Load();
    void Save(AppSettings settings);
}

/// <summary>
/// Persistance JSON simple. Si le fichier est absent ou corrompu, renvoie des valeurs par
/// défaut plutôt que de faire échouer le démarrage de l'application (§145) — un fichier de
/// paramètres illisible ne doit jamais empêcher d'utiliser le logiciel.
/// </summary>
public sealed class JsonSettingsStore(StoragePaths paths) : ISettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public AppSettings Load()
    {
        if (!File.Exists(paths.SettingsFile))
        {
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(paths.SettingsFile);
            return JsonSerializer.Deserialize<AppSettings>(json, Options) ?? new AppSettings();
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, Options);
        var tempFile = paths.SettingsFile + ".tmp";
        File.WriteAllText(tempFile, json);
        File.Move(tempFile, paths.SettingsFile, overwrite: true); // écriture atomique
    }
}
