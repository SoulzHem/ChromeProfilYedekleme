using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ChromeProfilApp.Models;
using Microsoft.Data.Sqlite;

namespace ChromeProfilApp.Services;

public sealed class ProfileAccountService
{
    private static readonly Regex EmailRegex = new(
        @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public List<ProfileAccount> GetAccounts(string profilePath, string? primaryEmail = null)
    {
        var googleAccounts = ReadGoogleAccounts(profilePath, primaryEmail);
        var googleEmails = new HashSet<string>(googleAccounts.Select(a => a.Email),
            StringComparer.OrdinalIgnoreCase);

        var siteEmails = ReadSiteLoginEmails(profilePath)
            .Where(e => !googleEmails.Contains(e))
            .Select(e => new ProfileAccount
            {
                Email = e,
                FullName = "-",
                GaiaId = "",
                IsPrimary = false,
                AccountKind = "Site Girişi"
            });

        return googleAccounts.Concat(siteEmails)
            .OrderByDescending(a => a.IsPrimary)
            .ThenBy(a => a.AccountKind)
            .ThenBy(a => a.Email, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<ProfileAccount> ReadGoogleAccounts(string profilePath, string? primaryEmail)
    {
        var prefPath = Path.Combine(profilePath, "Preferences");
        if (!File.Exists(prefPath)) return [];

        try
        {
            var temp = ChromeFileHelper.CopyToTemp(prefPath, "chrome_prefs");
            try
            {
                var root = JsonNode.Parse(File.ReadAllText(temp));
                var accountInfo = root?["account_info"]?.AsArray();
                if (accountInfo == null || accountInfo.Count == 0) return [];

                var byGaia = new Dictionary<string, ProfileAccount>(StringComparer.Ordinal);
                foreach (var node in accountInfo)
                {
                    if (node == null) continue;
                    var email = node["email"]?.GetValue<string>() ?? "";
                    if (string.IsNullOrWhiteSpace(email)) continue;

                    var gaia = node["gaia"]?.GetValue<string>() ?? node["account_id"]?.GetValue<string>() ?? "";
                    var acc = new ProfileAccount
                    {
                        Email = email,
                        FullName = node["full_name"]?.GetValue<string>() ?? "",
                        GaiaId = gaia,
                        IsPrimary = false,
                        AccountKind = "Google Hesabı"
                    };
                    if (!string.IsNullOrEmpty(gaia))
                        byGaia[gaia] = acc;
                    else
                        byGaia[email] = acc;
                }

                // metadata'da olup account_info'da gaia eşleşmesi olan hesapları da dahil et
                var metadata = root?["signin"]?["accounts_metadata_dict"]?.AsObject();
                if (metadata != null)
                {
                    foreach (var entry in metadata)
                    {
                        if (byGaia.ContainsKey(entry.Key)) continue;
                        // gaia metadata'da var ama account_info'da yok — Preferences içinde e-posta ara
                        var emailFromJson = FindEmailForGaia(root, entry.Key);
                        if (!string.IsNullOrWhiteSpace(emailFromJson))
                        {
                            byGaia[entry.Key] = new ProfileAccount
                            {
                                Email = emailFromJson,
                                FullName = "",
                                GaiaId = entry.Key,
                                IsPrimary = false,
                                AccountKind = "Google (oturum kaydı)"
                            };
                        }
                    }
                }

                var accounts = byGaia.Values
                    .GroupBy(a => a.Email, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

                if (accounts.Count == 0) return accounts;

                var primaryIndex = 0;
                if (!string.IsNullOrWhiteSpace(primaryEmail))
                {
                    var idx = accounts.FindIndex(a =>
                        a.Email.Equals(primaryEmail, StringComparison.OrdinalIgnoreCase));
                    if (idx >= 0) primaryIndex = idx;
                }

                return accounts
                    .Select((a, i) => new ProfileAccount
                    {
                        Email = a.Email,
                        FullName = a.FullName,
                        GaiaId = a.GaiaId,
                        IsPrimary = i == primaryIndex,
                        AccountKind = a.AccountKind
                    })
                    .OrderByDescending(a => a.IsPrimary)
                    .ThenBy(a => a.Email, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            finally
            {
                TryDelete(temp);
            }
        }
        catch
        {
            return [];
        }
    }

    private static string? FindEmailForGaia(JsonNode? root, string gaiaId)
    {
        if (root == null) return null;
        var json = root.ToJsonString();
        var pattern = $@"""gaia""\s*:\s*""{Regex.Escape(gaiaId)}""";
        var match = Regex.Match(json, pattern);
        if (!match.Success) return null;

        var start = Math.Max(0, match.Index - 400);
        var chunk = json.Substring(start, Math.Min(800, json.Length - start));
        var emailMatch = EmailRegex.Matches(chunk).Cast<Match>()
            .Select(m => m.Value)
            .LastOrDefault(e => !e.Contains("google.com", StringComparison.OrdinalIgnoreCase));
        return emailMatch;
    }

    private static IEnumerable<string> ReadSiteLoginEmails(string profilePath)
    {
        var loginDataPath = Path.Combine(profilePath, "Login Data");
        if (!File.Exists(loginDataPath)) yield break;

        var tempDb = ChromeFileHelper.CopyToTemp(loginDataPath, "chrome_login_emails");
        try
        {
            using var conn = new SqliteConnection($"Data Source={tempDb};Mode=ReadOnly");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT DISTINCT username_value FROM logins
                WHERE username_value LIKE '%@%'
                ORDER BY username_value
                """;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var value = reader.IsDBNull(0) ? "" : reader.GetString(0);
                if (EmailRegex.IsMatch(value))
                    yield return value.Trim();
            }
        }
        finally
        {
            TryDelete(tempDb);
        }
    }

    public static int CountGoogleAccounts(IReadOnlyList<ProfileAccount> accounts) =>
        accounts.Count(a => a.AccountKind.StartsWith("Google", StringComparison.OrdinalIgnoreCase));

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { /* ignore */ }
    }
}
