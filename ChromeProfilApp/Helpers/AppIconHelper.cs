namespace ChromeProfilApp.Helpers;

internal static class AppIconHelper
{
    public static Icon? Load()
    {
        try
        {
            var exe = Application.ExecutablePath;
            if (File.Exists(exe))
                return Icon.ExtractAssociatedIcon(exe);
        }
        catch
        {
            // ignore
        }

        return null;
    }

    public static void Apply(Form form)
    {
        var icon = Load();
        if (icon != null)
            form.Icon = icon;
    }
}
