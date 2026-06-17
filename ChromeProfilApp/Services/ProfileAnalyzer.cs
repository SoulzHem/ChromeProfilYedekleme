using System.Text.Json.Nodes;
using ChromeProfilApp.Models;

namespace ChromeProfilApp.Services;

public sealed class ProfileAnalyzer
{
    private static readonly HashSet<string> ExcludedDirNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cache", "Code Cache", "GPUCache", "GrShaderCache", "ShaderCache",
        "BrowserMetrics", "Crashpad", "component_crx_cache", "extensions_crx_cache",
        "CacheStorage", "ScriptCache"
    };

    private readonly ChromePasswordService _passwordService = new();
    private readonly BookmarkService _bookmarkService = new();

    public ChromeProfile EnrichProfile(ChromeProfile profile, JsonNode? profileInfo, bool includeSizes = true)
    {
        if (includeSizes)
        {
            profile.DiskSizeBytes = GetDirectorySize(profile.Path, excludeCache: false);
            profile.BackupSizeBytes = GetDirectorySize(profile.Path, excludeCache: true);
        }
        profile.BookmarkCount = _bookmarkService.CountBookmarks(profile.Path);
        profile.ExtensionCount = CountExtensions(profile.Path);
        profile.PasswordCount = _passwordService.GetPasswordCount(profile.Path);

        if (profileInfo?["active_time"] != null)
        {
            var unix = profileInfo["active_time"]!.GetValue<double>();
            profile.LastActive = DateTimeOffset.FromUnixTimeSeconds((long)unix).LocalDateTime;
        }

        return profile;
    }

    public static int CountExtensions(string profilePath)
    {
        var extPath = Path.Combine(profilePath, "Extensions");
        if (!Directory.Exists(extPath)) return 0;

        try
        {
            return Directory.GetDirectories(extPath).Length;
        }
        catch
        {
            return 0;
        }
    }

    public static long GetDirectorySize(string path, bool excludeCache)
    {
        try
        {
            long total = 0;
            var stack = new Stack<string>();
            stack.Push(path);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                foreach (var file in Directory.EnumerateFiles(current))
                {
                    try { total += new FileInfo(file).Length; }
                    catch { /* ignore locked files */ }
                }

                foreach (var dir in Directory.EnumerateDirectories(current))
                {
                    var name = Path.GetFileName(dir);
                    if (excludeCache && ShouldExcludeDir(name, dir))
                        continue;
                    stack.Push(dir);
                }
            }

            return total;
        }
        catch
        {
            return 0;
        }
    }

    private static bool ShouldExcludeDir(string name, string fullPath)
    {
        if (ExcludedDirNames.Contains(name)) return true;

        var parent = Path.GetFileName(Path.GetDirectoryName(fullPath) ?? "");
        if (parent.Equals("Service Worker", StringComparison.OrdinalIgnoreCase) &&
            (name.Equals("CacheStorage", StringComparison.OrdinalIgnoreCase) ||
             name.Equals("ScriptCache", StringComparison.OrdinalIgnoreCase)))
            return true;

        return false;
    }
}
