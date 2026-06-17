namespace ChromeProfilApp.Models;

public sealed class BackupPreviewSummary
{
    public int ProfileCount { get; init; }
    public long TotalBackupBytes { get; init; }
    public long TotalDiskBytes { get; init; }
    public long TotalCacheBytes { get; init; }
    public long LocalStateBytes { get; init; }
    public string BackupRoot { get; init; } = "";
    public string EstimatedFolderName { get; init; } = "";
    public string EstimatedFullPath { get; init; } = "";
    public List<ChromeProfile> Profiles { get; init; } = [];
    public BackupOptions Options { get; init; } = new();
}
