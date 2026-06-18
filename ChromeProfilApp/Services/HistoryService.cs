using ChromeProfilApp.Models;
using Microsoft.Data.Sqlite;

namespace ChromeProfilApp.Services;

public sealed class HistoryService
{
    public List<HistoryItem> GetTopHistory(string profilePath, int limit = 200)
    {
        var historyPath = Path.Combine(profilePath, "History");
        if (!File.Exists(historyPath)) return [];

        var tempDb = ChromeFileHelper.CopyToTemp(historyPath, "chrome_history");
        try
        {
            using var conn = new SqliteConnection($"Data Source={tempDb};Mode=ReadOnly");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT u.title, u.url, u.visit_count, u.last_visit_time
                FROM urls u
                ORDER BY u.last_visit_time DESC
                LIMIT @limit
                """;
            cmd.Parameters.AddWithValue("@limit", limit);

            var list = new List<HistoryItem>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var title = reader.IsDBNull(0) ? "" : reader.GetString(0);
                var url = reader.IsDBNull(1) ? "" : reader.GetString(1);
                var visits = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                DateTime? lastVisit = null;
                if (!reader.IsDBNull(3))
                    lastVisit = ChromeTimeToDateTime(reader.GetInt64(3));

                list.Add(new HistoryItem
                {
                    Title = string.IsNullOrWhiteSpace(title) ? url : title,
                    Url = url,
                    VisitCount = visits,
                    LastVisit = lastVisit
                });
            }

            return list;
        }
        catch
        {
            return [];
        }
        finally
        {
            TryDelete(tempDb);
        }
    }

    private static DateTime? ChromeTimeToDateTime(long chromeTime)
    {
        try
        {
            var unix = (chromeTime / 1_000_000) - 11_644_473_600;
            return DateTimeOffset.FromUnixTimeSeconds(unix).LocalDateTime;
        }
        catch
        {
            return null;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch
        {
            // Temp file may be in use
        }
    }
}
