using ChromeProfilApp.Services;

namespace ChromeProfilApp;

internal static class SilentBackupRunner
{
    public static int Run()
    {
        try
        {
            var service = new ChromeService();
            if (service.IsChromeRunning())
                return 2;

            var profiles = service.GetProfiles(includeSizes: false);
            if (profiles.Count == 0)
                return 3;

            var options = service.GetBackupOptions();
            service.BackupAll(profiles, options);
            return 0;
        }
        catch
        {
            return 1;
        }
    }
}
