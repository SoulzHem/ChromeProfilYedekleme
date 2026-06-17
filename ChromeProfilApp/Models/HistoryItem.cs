namespace ChromeProfilApp.Models;

public sealed class HistoryItem
{
    public string Title { get; init; } = "";
    public string Url { get; init; } = "";
    public int VisitCount { get; init; }
    public DateTime? LastVisit { get; init; }
}
