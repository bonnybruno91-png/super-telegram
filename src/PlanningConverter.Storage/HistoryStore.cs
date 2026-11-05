using Microsoft.Data.Sqlite;

namespace PlanningConverter.Storage;

/// <summary>
/// Une entrée d'historique. Ne contient JAMAIS le contenu du classeur source (§52) —
/// seulement des métadonnées et une empreinte SHA-256 du fichier, qui permet de détecter
/// qu'un même planning a déjà été converti (§100) sans avoir à en garder une copie.
/// </summary>
public sealed record HistoryEntry
{
    public required Guid Id { get; init; }
    public required string SourceFileName { get; init; }
    public required string SourceFileHash { get; init; }
    public string? Promotion { get; init; }
    public DateOnly? PeriodStart { get; init; }
    public DateOnly? PeriodEnd { get; init; }
    public required DateTimeOffset ConvertedAt { get; init; }
    public int EventCount { get; init; }
    public int ValidCount { get; init; }
    public int WarningCount { get; init; }
    public int ErrorCount { get; init; }
    public required IReadOnlyList<string> ExportedFormats { get; init; }
}

public interface IHistoryStore
{
    void Add(HistoryEntry entry);
    IReadOnlyList<HistoryEntry> ListRecent(int limit = 50);

    /// <summary>§100 : une conversion antérieure du même fichier existe-t-elle déjà ?</summary>
    HistoryEntry? FindByFileHash(string sourceFileHash);

    void Delete(Guid id);
}

/// <summary>Calcule l'empreinte utilisée pour la détection de reconversion (§100).</summary>
public static class FileHasher
{
    public static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(stream);
        return Convert.ToHexStringLower(hash);
    }
}

public sealed class SqliteHistoryStore : IHistoryStore
{
    private readonly string _connectionString;

    public SqliteHistoryStore(StoragePaths paths)
    {
        _connectionString = $"Data Source={paths.HistoryDatabaseFile}";
        EnsureSchema();
    }

    private void EnsureSchema()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS history (
                id TEXT PRIMARY KEY,
                source_file_name TEXT NOT NULL,
                source_file_hash TEXT NOT NULL,
                promotion TEXT,
                period_start TEXT,
                period_end TEXT,
                converted_at TEXT NOT NULL,
                event_count INTEGER NOT NULL,
                valid_count INTEGER NOT NULL,
                warning_count INTEGER NOT NULL,
                error_count INTEGER NOT NULL,
                exported_formats TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_history_hash ON history(source_file_hash);
            CREATE INDEX IF NOT EXISTS idx_history_converted_at ON history(converted_at);
            """;
        cmd.ExecuteNonQuery();
    }

    public void Add(HistoryEntry entry)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO history
                (id, source_file_name, source_file_hash, promotion, period_start, period_end,
                 converted_at, event_count, valid_count, warning_count, error_count, exported_formats)
            VALUES
                ($id, $name, $hash, $promotion, $start, $end,
                 $convertedAt, $eventCount, $validCount, $warningCount, $errorCount, $formats)
            """;
        cmd.Parameters.AddWithValue("$id", entry.Id.ToString());
        cmd.Parameters.AddWithValue("$name", entry.SourceFileName);
        cmd.Parameters.AddWithValue("$hash", entry.SourceFileHash);
        cmd.Parameters.AddWithValue("$promotion", (object?)entry.Promotion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$start", (object?)entry.PeriodStart?.ToString("yyyy-MM-dd") ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$end", (object?)entry.PeriodEnd?.ToString("yyyy-MM-dd") ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$convertedAt", entry.ConvertedAt.ToString("O"));
        cmd.Parameters.AddWithValue("$eventCount", entry.EventCount);
        cmd.Parameters.AddWithValue("$validCount", entry.ValidCount);
        cmd.Parameters.AddWithValue("$warningCount", entry.WarningCount);
        cmd.Parameters.AddWithValue("$errorCount", entry.ErrorCount);
        cmd.Parameters.AddWithValue("$formats", string.Join(",", entry.ExportedFormats));
        cmd.ExecuteNonQuery();
    }

    public IReadOnlyList<HistoryEntry> ListRecent(int limit = 50)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM history ORDER BY converted_at DESC LIMIT $limit";
        cmd.Parameters.AddWithValue("$limit", limit);
        using var reader = cmd.ExecuteReader();
        var results = new List<HistoryEntry>();
        while (reader.Read())
        {
            results.Add(ReadEntry(reader));
        }
        return results;
    }

    public HistoryEntry? FindByFileHash(string sourceFileHash)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM history WHERE source_file_hash = $hash ORDER BY converted_at DESC LIMIT 1";
        cmd.Parameters.AddWithValue("$hash", sourceFileHash);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? ReadEntry(reader) : null;
    }

    public void Delete(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM history WHERE id = $id";
        cmd.Parameters.AddWithValue("$id", id.ToString());
        cmd.ExecuteNonQuery();
    }

    private static HistoryEntry ReadEntry(SqliteDataReader reader) => new()
    {
        Id = Guid.Parse(reader.GetString(reader.GetOrdinal("id"))),
        SourceFileName = reader.GetString(reader.GetOrdinal("source_file_name")),
        SourceFileHash = reader.GetString(reader.GetOrdinal("source_file_hash")),
        Promotion = reader.IsDBNull(reader.GetOrdinal("promotion")) ? null : reader.GetString(reader.GetOrdinal("promotion")),
        PeriodStart = reader.IsDBNull(reader.GetOrdinal("period_start")) ? null : DateOnly.Parse(reader.GetString(reader.GetOrdinal("period_start"))),
        PeriodEnd = reader.IsDBNull(reader.GetOrdinal("period_end")) ? null : DateOnly.Parse(reader.GetString(reader.GetOrdinal("period_end"))),
        ConvertedAt = DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("converted_at"))),
        EventCount = reader.GetInt32(reader.GetOrdinal("event_count")),
        ValidCount = reader.GetInt32(reader.GetOrdinal("valid_count")),
        WarningCount = reader.GetInt32(reader.GetOrdinal("warning_count")),
        ErrorCount = reader.GetInt32(reader.GetOrdinal("error_count")),
        ExportedFormats = reader.GetString(reader.GetOrdinal("exported_formats")).Split(',', StringSplitOptions.RemoveEmptyEntries),
    };
}
