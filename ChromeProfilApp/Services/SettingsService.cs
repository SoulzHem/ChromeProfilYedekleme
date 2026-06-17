using System.Text.Json;
using ChromeProfilApp.Models;

namespace ChromeProfilApp.Services;

public sealed class SettingsService
{
    private readonly string _settingsPath;

    public SettingsService()
    {
        var exeDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        _settingsPath = Path.Combine(exeDir, "ayarlar.json");
    }

    public AppSettings Load(string defaultBackupRoot)
    {
        if (!File.Exists(_settingsPath))
            return new AppSettings { BackupRoot = defaultBackupRoot };

        try
        {
            var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath));
            if (settings == null || string.IsNullOrWhiteSpace(settings.BackupRoot))
                return new AppSettings { BackupRoot = defaultBackupRoot };

            return settings;
        }
        catch
        {
            return new AppSettings { BackupRoot = defaultBackupRoot };
        }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsPath, json);
    }
}
