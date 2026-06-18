using System.Text.Json;
using System.Text.Json.Nodes;
using ChromeProfilApp.Models;
using Microsoft.Data.Sqlite;

namespace ChromeProfilApp.Services;

public sealed class ProfileStatsService
{
    public ProfileStats GetStats(string profilePath, string chromeUserData)
    {
        var stats = new ProfileStats { ProfilePath = profilePath };

        stats.ChromeVersion = ReadChromeVersion(chromeUserData);
        stats.HistoryCount = CountHistory(profilePath);
        stats.OpenTabCount = CountOpenTabs(profilePath);

        return stats;
    }

    public List<ExtensionInfo> GetExtensions(string profilePath)
    {
        var extRoot = Path.Combine(profilePath, "Extensions");
        if (!Directory.Exists(extRoot)) return [];

        var list = new List<ExtensionInfo>();
        foreach (var extDir in Directory.GetDirectories(extRoot))
        {
            var id = Path.GetFileName(extDir);
            var versionDirs = Directory.GetDirectories(extDir);
            if (versionDirs.Length == 0) continue;

            var manifestPath = Path.Combine(versionDirs[^1], "manifest.json");
            if (!File.Exists(manifestPath)) continue;

            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(manifestPath));
                var root = doc.RootElement;
                var name = root.TryGetProperty("name", out var nameProp)
                    ? nameProp.GetString() ?? id
                    : id;
                if (name.StartsWith("__MSG_") && root.TryGetProperty("default_locale", out _))
                    name = id;

                var version = root.TryGetProperty("version", out var verProp)
                    ? verProp.GetString() ?? "?"
                    : "?";

                list.Add(new ExtensionInfo { Id = id, Name = name, Version = version });
            }
            catch
            {
                list.Add(new ExtensionInfo { Id = id, Name = id, Version = "?" });
            }
        }

        return list.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string ReadChromeVersion(string chromeUserData)
    {
        var localState = Path.Combine(chromeUserData, "Local State");
        if (!File.Exists(localState)) return "-";

        try
        {
            var root = JsonNode.Parse(File.ReadAllText(localState));
            return root?["browser"]?["last_version"]?.GetValue<string>() ?? "-";
        }
        catch
        {
            return "-";
        }
    }

    private static int CountHistory(string profilePath)
    {
        var historyPath = Path.Combine(profilePath, "History");
        if (!File.Exists(historyPath)) return 0;

        var tempDb = ChromeFileHelper.CopyToTemp(historyPath, "chrome_history");
        try
        {
            using var conn = new SqliteConnection($"Data Source={tempDb};Mode=ReadOnly");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM urls";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        catch
        {
            return 0;
        }
        finally
        {
            TryDelete(tempDb);
        }
    }

    private static int CountOpenTabs(string profilePath)
    {
        var sessionPath = Path.Combine(profilePath, "Sessions");
        if (!Directory.Exists(sessionPath)) return 0;

        try
        {
            return Directory.GetFiles(sessionPath, "Session_*").Length +
                   Directory.GetFiles(sessionPath, "Tabs_*").Length;
        }
        catch
        {
            return 0;
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
            // File may be in use or already deleted by another process
        }
    }
}
