namespace ChromeProfilApp.Models;

public sealed class BackupOptions
{
    public bool IncludePasswords { get; set; } = true;
    public bool IncludeHistory { get; set; } = true;
    public bool IncludeCookies { get; set; } = true;
    public bool IncludeExtensions { get; set; } = true;
    public bool IncludeCache { get; set; } = false;

    public List<string> GetIncludedParts()
    {
        var parts = new List<string> { "Yer imleri ve ayarlar", "Local State", "Tercihler (Preferences)" };
        if (IncludePasswords) parts.Add("Kayıtlı şifreler");
        if (IncludeHistory) parts.Add("Geçmiş");
        if (IncludeCookies) parts.Add("Çerezler ve oturum");
        if (IncludeExtensions) parts.Add("Eklentiler");
        if (IncludeCache) parts.Add("Önbellek (cache)");
        return parts;
    }

    public List<string> GetExcludedParts()
    {
        var parts = new List<string>();
        if (!IncludePasswords) parts.Add("Şifreler (Login Data)");
        if (!IncludeHistory) parts.Add("Geçmiş (History, Top Sites)");
        if (!IncludeCookies) parts.Add("Çerezler (Cookies, Network)");
        if (!IncludeExtensions) parts.Add("Eklentiler (Extensions)");
        if (!IncludeCache) parts.Add("Önbellek (Cache, GPUCache...)");
        return parts;
    }
}
