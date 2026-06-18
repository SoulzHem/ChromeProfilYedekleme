using System.IO;

namespace ChromeProfilApp;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) => LogException(e.ExceptionObject as Exception, "UnhandledException");
        Application.ThreadException += (_, e) => LogException(e.Exception, "ThreadException");

        try
        {
            // System.Data.SQLite initializes itself automatically.

            if (args.Contains("--otomatik-yedek", StringComparer.OrdinalIgnoreCase))
            {
                Environment.Exit(SilentBackupRunner.Run());
                return;
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            LogException(ex, "MainException");
            throw;
        }
    }

    private static void LogException(Exception? ex, string kind)
    {
        try
        {
            if (ex == null) return;
            var path = Path.Combine(AppContext.BaseDirectory, "startup-error.log");
            File.AppendAllText(path, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {kind}: {ex}\r\n\r\n");
        }
        catch
        {
            // ignore logging failures
        }
    }
}
