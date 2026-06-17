using System.Text;
using ChromeProfilApp.Models;

namespace ChromeProfilApp.Services;

public sealed class ExportService
{
    public static void ExportBookmarksToCsv(IEnumerable<BookmarkItem> items, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Baslik,Klasor,Adres");
        foreach (var b in items)
            sb.AppendLine($"{Csv(b.Title)},{Csv(b.Folder)},{Csv(b.Url)}");
        File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
    }

    public static void ExportPasswordsToCsv(IEnumerable<SavedPassword> items, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Site,Kullanici,Sifre");
        foreach (var p in items)
            sb.AppendLine($"{Csv(p.Site)},{Csv(p.Username)},{Csv(p.Password)}");
        File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
    }

    public static void ExportAccountsToCsv(IEnumerable<ProfileAccount> items, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Eposta,Ad,Tur,AnaHesap");
        foreach (var a in items)
            sb.AppendLine($"{Csv(a.Email)},{Csv(a.FullName)},{Csv(a.AccountKind)},{(a.IsPrimary ? "Evet" : "Hayir")}");
        File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
    }

    public static void ExportHistoryToCsv(IEnumerable<HistoryItem> items, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Baslik,Adres,Ziyaret,SonZiyaret");
        foreach (var h in items)
            sb.AppendLine($"{Csv(h.Title)},{Csv(h.Url)},{h.VisitCount},{h.LastVisit:dd.MM.yyyy HH:mm}");
        File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
    }

    private static string Csv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
