namespace ChromeProfilApp;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Contains("--otomatik-yedek", StringComparer.OrdinalIgnoreCase))
        {
            Environment.Exit(SilentBackupRunner.Run());
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
