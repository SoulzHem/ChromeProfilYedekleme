using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using ChromeProfilApp.Models;

namespace ChromeProfilApp.Services;

public sealed class ChromeService
{
    private static readonly string[] ExcludedSystemProfiles = ["System Profile", "Guest Profile"];

    private static readonly string[] RobocopyExcludeDirs =
    [
        "Cache", "Code Cache", "GPUCache", "GrShaderCache", "ShaderCache",
        "BrowserMetrics", "Crashpad", "component_crx_cache", "extensions_crx_cache"
    ];

    public static IReadOnlyList<string> BackupIncludes { get; } =
    [
        "Local State (profil listesi, isimler, sira)",
        "Profil klasorleri (Default, Profile 1, ...)",
        "Yer imleri, ayarlar, oturum verileri",
        "Eklentiler ve eklenti ayarlari",
        "Kayitli sifreler (Login Data)",
        "Gecmis, cerezler, tercihler",
        "manifest.json (yedek ozeti)"
    ];

    public static IReadOnlyList<string> BackupExcludes { get; } =
    [
        "Cache, Code Cache, GPUCache",
        "ShaderCache, GrShaderCache",
        "Service Worker onbellegi",
        "BrowserMetrics, Crashpad",
        "component_crx_cache, extensions_crx_cache"
    ];

    private readonly ProfileAnalyzer _analyzer = new();
    private readonly ChromePasswordService _passwordService = new();
    private readonly BookmarkService _bookmarkService = new();
    private readonly ProfileAccountService _accountService = new();
    private readonly ProfileStatsService _statsService = new();
    private readonly HistoryService _historyService = new();
    private readonly SettingsService _settingsService = new();

    public string ChromeUserData { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Google", "Chrome", "User Data");

    public string DefaultBackupRoot { get; }

    public string BackupRoot { get; private set; }

    public ChromeService()
    {
        var exeDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        DefaultBackupRoot = Path.Combine(exeDir, "Yedekler");
        BackupRoot = _settingsService.Load(DefaultBackupRoot).BackupRoot;
    }

    public void SetBackupRoot(string path)
    {
        BackupRoot = path;
        var settings = _settingsService.Load(DefaultBackupRoot);
        settings.BackupRoot = path;
        _settingsService.Save(settings);
    }

    public BackupOptions GetBackupOptions()
    {
        return _settingsService.Load(DefaultBackupRoot).BackupOptions ?? new BackupOptions();
    }

    public void SaveBackupOptions(BackupOptions options)
    {
        var settings = _settingsService.Load(DefaultBackupRoot);
        settings.BackupOptions = options;
        _settingsService.Save(settings);
    }

    public bool IsChromeRunning() =>
        Process.GetProcessesByName("chrome").Length > 0;

    public bool StopChrome(IProgress<string>? progress = null)
    {
        if (!IsChromeRunning()) return true;

        progress?.Report("Chrome kapatılıyor...");
        foreach (var proc in Process.GetProcessesByName("chrome"))
        {
            try { proc.Kill(); }
            catch { /* ignore */ }
        }

        Thread.Sleep(3000);
        return !IsChromeRunning();
    }

    public List<ChromeProfile> GetProfiles(bool includeSizes = true)
    {
        var localStatePath = Path.Combine(ChromeUserData, "Local State");
        if (!File.Exists(localStatePath))
            throw new FileNotFoundException("Chrome Local State bulunamadı. Chrome kurulu mu?", localStatePath);

        var json = File.ReadAllText(localStatePath);
        var root = JsonNode.Parse(json) ?? throw new InvalidOperationException("Local State okunamadı.");
        var infoCache = root["profile"]?["info_cache"]?.AsObject()
            ?? throw new InvalidOperationException("Profil bilgisi bulunamadı.");

        var profiles = new List<ChromeProfile>();

        foreach (var entry in infoCache)
        {
            var folder = entry.Key;
            if (ExcludedSystemProfiles.Contains(folder)) continue;

            var profilePath = Path.Combine(ChromeUserData, folder);
            if (!Directory.Exists(profilePath)) continue;

            var info = entry.Value;
            var name = info?["name"]?.GetValue<string>() ?? folder;
            var email = info?["user_name"]?.GetValue<string>() ?? "-";

            var profile = new ChromeProfile
            {
                Folder = folder,
                Name = name,
                Email = email,
                Path = profilePath
            };

            profile = _analyzer.EnrichProfile(profile, info, includeSizes);
            profile.Accounts = _accountService.GetAccounts(profilePath, email);
            profiles.Add(profile);
        }

        return profiles.OrderBy(p => p.Folder, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public List<SavedPassword> GetProfilePasswords(ChromeProfile profile) =>
        _passwordService.GetPasswords(profile.Path, ChromeUserData);

    public List<BookmarkItem> GetProfileBookmarks(ChromeProfile profile) =>
        _bookmarkService.GetBookmarks(profile.Path);

    public List<ProfileAccount> GetProfileAccounts(ChromeProfile profile) =>
        profile.Accounts.Count > 0
            ? profile.Accounts
            : _accountService.GetAccounts(profile.Path, profile.Email);

    public ProfileStats GetProfileStats(ChromeProfile profile) =>
        _statsService.GetStats(profile.Path, ChromeUserData);

    public List<ExtensionInfo> GetProfileExtensions(ChromeProfile profile) =>
        _statsService.GetExtensions(profile.Path);

    public List<HistoryItem> GetProfileHistory(ChromeProfile profile, int limit = 200) =>
        _historyService.GetTopHistory(profile.Path, limit);

    public void EnrichProfileSizes(ChromeProfile profile)
    {
        profile.DiskSizeBytes = ProfileAnalyzer.GetDirectorySize(profile.Path, excludeCache: false);
        profile.BackupSizeBytes = ProfileAnalyzer.GetDirectorySize(profile.Path, excludeCache: true);
    }

    public BackupPreviewSummary BuildBackupPreview(
        IReadOnlyList<ChromeProfile> profiles,
        BackupOptions options,
        string? backupRoot = null)
    {
        backupRoot ??= BackupRoot;
        var folderName = $"ChromeYedek_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";

        long localStateBytes = 0;
        var localStatePath = Path.Combine(ChromeUserData, "Local State");
        if (File.Exists(localStatePath))
            localStateBytes = new FileInfo(localStatePath).Length;

        var totalBackup = profiles.Sum(p => p.BackupSizeBytes) + localStateBytes;
        var totalDisk = profiles.Sum(p => p.DiskSizeBytes) + localStateBytes;

        return new BackupPreviewSummary
        {
            ProfileCount = profiles.Count,
            TotalBackupBytes = totalBackup,
            TotalDiskBytes = totalDisk,
            TotalCacheBytes = Math.Max(0, totalDisk - totalBackup),
            LocalStateBytes = localStateBytes,
            BackupRoot = backupRoot,
            EstimatedFolderName = folderName,
            EstimatedFullPath = Path.Combine(backupRoot, folderName),
            Profiles = profiles.ToList(),
            Options = options
        };
    }

    public List<BackupInfo> GetBackups(string? backupRoot = null)
    {
        backupRoot ??= BackupRoot;
        if (!Directory.Exists(backupRoot)) return [];

        return Directory.GetDirectories(backupRoot)
            .Select(dir =>
            {
                var manifestPath = Path.Combine(dir, "manifest.json");
                if (!File.Exists(manifestPath)) return null;

                try
                {
                    var manifest = JsonSerializer.Deserialize<BackupManifest>(File.ReadAllText(manifestPath));
                    if (manifest == null) return null;

                    return new BackupInfo
                    {
                        FolderPath = dir,
                        FolderName = Path.GetFileName(dir),
                        Created = manifest.Created,
                        ProfileCount = manifest.ProfileCount,
                        Computer = manifest.Computer
                    };
                }
                catch
                {
                    return null;
                }
            })
            .Where(b => b != null)
            .Cast<BackupInfo>()
            .OrderByDescending(b => b.Created)
            .ToList();
    }

    public string BackupAll(
        IReadOnlyList<ChromeProfile> profiles,
        BackupOptions options,
        IProgress<string>? progress = null,
        IProgress<int>? percent = null,
        string? backupRoot = null)
    {
        backupRoot ??= BackupRoot;
        Directory.CreateDirectory(backupRoot);

        if (!Directory.Exists(ChromeUserData))
            throw new DirectoryNotFoundException($"Chrome verisi bulunamadı: {ChromeUserData}");

        if (profiles.Count == 0)
            throw new InvalidOperationException("Yedeklenecek profil seçilmedi.");

        if (!StopChrome(progress))
            throw new InvalidOperationException("Chrome kapatılamadı. Görev Yöneticisinden kapatıp tekrar deneyin.");

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var backupDir = Path.Combine(backupRoot, $"ChromeYedek_{timestamp}");
        Directory.CreateDirectory(backupDir);

        progress?.Report("Local State yedekleniyor...");
        File.Copy(Path.Combine(ChromeUserData, "Local State"), Path.Combine(backupDir, "Local State"), true);
        percent?.Report(5);

        var manifestProfiles = new List<ManifestProfile>();
        var total = profiles.Count;

        for (var i = 0; i < total; i++)
        {
            var profile = profiles[i];
            progress?.Report($"[{i + 1}/{total}] {profile.Name} yedekleniyor...");
            var dest = Path.Combine(backupDir, profile.Folder);
            CopyFolderRobocopy(profile.Path, dest, options);
            BackupExclusionHelper.ApplyToProfile(dest, options);
            manifestProfiles.Add(new ManifestProfile
            {
                folder = profile.Folder,
                name = profile.Name,
                email = profile.Email
            });
            percent?.Report(5 + (int)((i + 1) / (double)total * 90));
        }

        var manifest = new BackupManifest
        {
            Version = "1.1",
            Created = DateTime.Now,
            Computer = Environment.MachineName,
            User = Environment.UserName,
            ChromePath = ChromeUserData,
            ProfileCount = manifestProfiles.Count,
            Profiles = manifestProfiles,
            Options = options
        };

        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(backupDir, "manifest.json"), json);

        SaveBackupOptions(options);
        percent?.Report(100);
        progress?.Report("Yedekleme tamamlandı!");
        return backupDir;
    }

    public void Restore(
        string backupPath,
        IReadOnlyList<string>? profileFolders = null,
        IProgress<string>? progress = null,
        IProgress<int>? percent = null)
    {
        var manifestPath = Path.Combine(backupPath, "manifest.json");
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("Geçerli yedek değil (manifest.json yok).", manifestPath);

        var manifest = JsonSerializer.Deserialize<BackupManifest>(File.ReadAllText(manifestPath))
            ?? throw new InvalidOperationException("manifest.json okunamadı.");

        var toRestore = manifest.Profiles.AsEnumerable();
        if (profileFolders is { Count: > 0 })
            toRestore = toRestore.Where(p => profileFolders.Contains(p.folder, StringComparer.OrdinalIgnoreCase));

        var profileList = toRestore.ToList();
        if (profileList.Count == 0)
            throw new InvalidOperationException("Geri yüklenecek profil seçilmedi.");

        if (!StopChrome(progress))
            throw new InvalidOperationException("Chrome kapatılamadı. Görev Yöneticisinden kapatıp tekrar deneyin.");

        Directory.CreateDirectory(ChromeUserData);

        progress?.Report("Local State geri yükleniyor...");
        File.Copy(Path.Combine(backupPath, "Local State"), Path.Combine(ChromeUserData, "Local State"), true);
        percent?.Report(10);

        var total = profileList.Count;
        for (var i = 0; i < total; i++)
        {
            var p = profileList[i];
            var src = Path.Combine(backupPath, p.folder);
            var dst = Path.Combine(ChromeUserData, p.folder);

            if (!Directory.Exists(src))
            {
                progress?.Report($"Atlandı (yedekte yok): {p.folder}");
                continue;
            }

            progress?.Report($"[{i + 1}/{total}] {p.name} geri yükleniyor...");
            CopyFolderRobocopy(src, dst, manifest.Options ?? new BackupOptions());
            percent?.Report(10 + (int)((i + 1) / (double)total * 85));
        }

        var firstRun = Path.Combine(ChromeUserData, "First Run");
        if (File.Exists(firstRun)) File.Delete(firstRun);

        percent?.Report(100);
        progress?.Report("Geri yükleme tamamlandı!");
    }

    private static void CopyFolderRobocopy(string source, string destination, BackupOptions? options = null)
    {
        options ??= new BackupOptions();
        Directory.CreateDirectory(destination);

        var psi = new ProcessStartInfo
        {
            FileName = "robocopy.exe",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        psi.ArgumentList.Add(source);
        psi.ArgumentList.Add(destination);
        psi.ArgumentList.Add("/E");
        psi.ArgumentList.Add("/COPY:DAT");
        psi.ArgumentList.Add("/R:2");
        psi.ArgumentList.Add("/W:2");
        psi.ArgumentList.Add("/NFL");
        psi.ArgumentList.Add("/NDL");
        psi.ArgumentList.Add("/NJH");
        psi.ArgumentList.Add("/NJS");
        psi.ArgumentList.Add("/NC");
        psi.ArgumentList.Add("/NS");
        psi.ArgumentList.Add("/NP");

        var excludeDirs = BackupExclusionHelper.GetRobocopyExcludeDirs(options);
        if (excludeDirs.Count > 0)
        {
            psi.ArgumentList.Add("/XD");
            foreach (var dir in excludeDirs)
                psi.ArgumentList.Add(dir);
            psi.ArgumentList.Add(@"Service Worker\CacheStorage");
            psi.ArgumentList.Add(@"Service Worker\ScriptCache");
        }

        using var proc = Process.Start(psi) ?? throw new InvalidOperationException("robocopy başlatılamadı.");
        proc.WaitForExit();

        if (proc.ExitCode > 7)
            throw new IOException($"Kopyalama hatası (robocopy: {proc.ExitCode}): {source}");
    }

    public static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double size = bytes;
        var i = 0;
        while (size >= 1024 && i < units.Length - 1)
        {
            size /= 1024;
            i++;
        }
        return $"{size:0.##} {units[i]}";
    }
}
