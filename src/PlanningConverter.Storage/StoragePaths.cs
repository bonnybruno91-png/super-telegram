namespace PlanningConverter.Storage;

/// <summary>
/// Centralise l'emplacement de toutes les données persistées, pour que le passage en
/// mode portable (§119) — tout dans un sous-dossier "data" à côté de l'exécutable —
/// ne touche qu'un seul endroit du code.
/// </summary>
public sealed class StoragePaths
{
    public string SettingsFile { get; }
    public string TemplatesFile { get; }
    public string HistoryDatabaseFile { get; }
    public string AutosaveFolder { get; }
    public string LogsFolder { get; }

    /// <param name="portableRoot">
    /// Si fourni, toutes les données vont dans ce dossier (mode portable, à côté de
    /// l'exécutable). Sinon, %APPDATA% et %LOCALAPPDATA% standards.
    /// </param>
    public StoragePaths(string? portableRoot = null)
    {
        string roamingRoot, localRoot;

        if (portableRoot is not null)
        {
            roamingRoot = Path.Combine(portableRoot, "data");
            localRoot = Path.Combine(portableRoot, "data");
        }
        else
        {
            roamingRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PlanningConverter");
            localRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlanningConverter");
        }

        SettingsFile = Path.Combine(roamingRoot, "settings.json");
        TemplatesFile = Path.Combine(roamingRoot, "templates.json");
        HistoryDatabaseFile = Path.Combine(localRoot, "history.db");
        AutosaveFolder = Path.Combine(localRoot, "autosave");
        LogsFolder = Path.Combine(localRoot, "logs");

        Directory.CreateDirectory(Path.GetDirectoryName(SettingsFile)!);
        Directory.CreateDirectory(Path.GetDirectoryName(HistoryDatabaseFile)!);
        Directory.CreateDirectory(AutosaveFolder);
        Directory.CreateDirectory(LogsFolder);
    }
}
