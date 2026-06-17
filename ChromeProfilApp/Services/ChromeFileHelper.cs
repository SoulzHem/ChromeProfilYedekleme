namespace ChromeProfilApp.Services;

internal static class ChromeFileHelper
{
    public static void CopyLockedFile(string source, string destination)
    {
        var dir = Path.GetDirectoryName(destination);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using var src = new FileStream(source, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        using var dst = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);
        src.CopyTo(dst);
    }

    public static string CopyToTemp(string sourceFile, string prefix)
    {
        var ext = Path.GetExtension(sourceFile);
        var temp = Path.Combine(Path.GetTempPath(), $"{prefix}_{Guid.NewGuid():N}{ext}");
        CopyLockedFile(sourceFile, temp);
        return temp;
    }
}
