namespace ChromeProfilApp.Models;

public sealed class ChromeProfile
{
    public string Folder { get; init; } = "";
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
    public string Path { get; init; } = "";
    public long DiskSizeBytes { get; set; }
    public long BackupSizeBytes { get; set; }
    public int BookmarkCount { get; set; }
    public int PasswordCount { get; set; }
    public int ExtensionCount { get; set; }
    public DateTime? LastActive { get; set; }
    public List<ProfileAccount> Accounts { get; set; } = [];

    public string EmailDisplay
    {
        get
        {
            if (Accounts.Count == 0) return Email;

            var primary = Accounts.FirstOrDefault(a => a.IsPrimary)?.Email
                ?? Accounts.FirstOrDefault(a => a.AccountKind.StartsWith("Google"))?.Email
                ?? Accounts[0].Email;

            var googleCount = Accounts.Count(a => a.AccountKind.StartsWith("Google", StringComparison.OrdinalIgnoreCase));
            var siteCount = Accounts.Count(a => a.AccountKind == "Site Girişi");
            var extras = new List<string>();
            if (googleCount > 1) extras.Add($"{googleCount - 1} Google");
            if (siteCount > 0) extras.Add($"{siteCount} site");

            return extras.Count == 0 ? primary : $"{primary} (+{string.Join(", +", extras)})";
        }
    }
}

public sealed class SavedPassword
{
    public string Site { get; init; } = "";
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
}

public sealed class BackupInfo
{
    public string FolderPath { get; init; } = "";
    public string FolderName { get; init; } = "";
    public DateTime Created { get; init; }
    public int ProfileCount { get; init; }
    public string Computer { get; init; } = "";
}

public sealed class BackupManifest
{
    public string Version { get; set; } = "1.0";
    public DateTime Created { get; set; }
    public string Computer { get; set; } = "";
    public string User { get; set; } = "";
    public string ChromePath { get; set; } = "";
    public int ProfileCount { get; set; }
    public List<ManifestProfile> Profiles { get; set; } = [];
    public BackupOptions? Options { get; set; }
}

public sealed class ManifestProfile
{
    public string folder { get; set; } = "";
    public string name { get; set; } = "";
    public string email { get; set; } = "";
}

public sealed class AppSettings
{
    public string BackupRoot { get; set; } = "";
    public BackupOptions BackupOptions { get; set; } = new();
}
