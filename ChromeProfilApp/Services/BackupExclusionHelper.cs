using ChromeProfilApp.Models;

namespace ChromeProfilApp.Services;

internal static class BackupExclusionHelper
{
    private static readonly string[] PasswordFiles =
        ["Login Data", "Login Data-journal", "Login Data For Account", "Login Data For Account-journal"];

    private static readonly string[] HistoryFiles =
    [
        "History", "History-journal", "Top Sites", "Top Sites-journal",
        "Visited Links", "Favicons", "Favicons-journal", "Shortcuts", "Shortcuts-journal"
    ];

    private static readonly string[] CookieFiles =
        ["Cookies", "Cookies-journal", "Network", "Safe Browsing Cookies", "Safe Browsing Cookies-journal"];

    public static void ApplyToProfile(string profileBackupPath, BackupOptions options)
    {
        if (!options.IncludePasswords)
            DeleteItems(profileBackupPath, PasswordFiles);

        if (!options.IncludeHistory)
            DeleteItems(profileBackupPath, HistoryFiles);

        if (!options.IncludeCookies)
            DeleteItems(profileBackupPath, CookieFiles);

        if (!options.IncludeExtensions)
            DeleteDirectory(Path.Combine(profileBackupPath, "Extensions"));
    }

    public static IReadOnlyList<string> GetRobocopyExcludeDirs(BackupOptions options)
    {
        if (options.IncludeCache)
            return [];

        return
        [
            "Cache", "Code Cache", "GPUCache", "GrShaderCache", "ShaderCache",
            "BrowserMetrics", "Crashpad", "component_crx_cache", "extensions_crx_cache"
        ];
    }

    private static void DeleteItems(string profilePath, IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            var path = Path.Combine(profilePath, name);
            try
            {
                if (File.Exists(path)) File.Delete(path);
                else if (Directory.Exists(path)) Directory.Delete(path, true);
            }
            catch { /* ignore locked files */ }
        }
    }

    private static void DeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        catch { /* ignore */ }
    }
}
