namespace ChromeProfilApp.Models;

public sealed class ProfileAccount
{
    public string Email { get; init; } = "";
    public string FullName { get; init; } = "";
    public string GaiaId { get; init; } = "";
    public bool IsPrimary { get; init; }
    public string AccountKind { get; init; } = "Google";
}

public sealed class ExtensionInfo
{
    public string Name { get; init; } = "";
    public string Id { get; init; } = "";
    public string Version { get; init; } = "";
}

public sealed class ProfileStats
{
    public int HistoryCount { get; set; }
    public int OpenTabCount { get; set; }
    public string ChromeVersion { get; set; } = "";
    public string ProfilePath { get; set; } = "";
}
