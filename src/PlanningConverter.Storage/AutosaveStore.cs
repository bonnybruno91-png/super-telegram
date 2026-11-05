namespace PlanningConverter.Storage;

/// <summary>
/// Persiste périodiquement l'état d'édition en cours pour permettre une reprise après
/// fermeture inattendue (§105). Volontairement agnostique du contenu exact : la couche
/// Application sérialise sa session en JSON et ce store se contente de l'écrire/lire —
/// ça évite à Storage de dépendre du modèle de session applicatif.
/// </summary>
public interface IAutosaveStore
{
    void Save(string sessionId, string sessionJson);
    string? TryLoad(string sessionId);
    void Clear(string sessionId);
    IReadOnlyList<string> ListPendingSessions();
}

public sealed class FileAutosaveStore(StoragePaths paths) : IAutosaveStore
{
    private string PathFor(string sessionId) => Path.Combine(paths.AutosaveFolder, $"{sessionId}.json");

    public void Save(string sessionId, string sessionJson)
    {
        var path = PathFor(sessionId);
        var tempFile = path + ".tmp";
        File.WriteAllText(tempFile, sessionJson);
        File.Move(tempFile, path, overwrite: true);
    }

    public string? TryLoad(string sessionId)
    {
        var path = PathFor(sessionId);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    public void Clear(string sessionId)
    {
        var path = PathFor(sessionId);
        if (File.Exists(path)) File.Delete(path);
    }

    public IReadOnlyList<string> ListPendingSessions() =>
        Directory.Exists(paths.AutosaveFolder)
            ? Directory.GetFiles(paths.AutosaveFolder, "*.json").Select(Path.GetFileNameWithoutExtension).Where(n => n is not null).Select(n => n!).ToList()
            : [];
}
