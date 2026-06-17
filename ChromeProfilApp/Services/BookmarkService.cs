using System.Text.Json.Nodes;
using ChromeProfilApp.Models;

namespace ChromeProfilApp.Services;

public sealed class BookmarkService
{
    public int CountBookmarks(string profilePath) => GetBookmarks(profilePath).Count;

    public List<BookmarkItem> GetBookmarks(string profilePath)
    {
        var bookmarksPath = Path.Combine(profilePath, "Bookmarks");
        if (!File.Exists(bookmarksPath)) return [];

        try
        {
            var temp = ChromeFileHelper.CopyToTemp(bookmarksPath, "chrome_bookmarks");
            try
            {
                var json = File.ReadAllText(temp);
                var root = JsonNode.Parse(json);
                var roots = root?["roots"];
                if (roots == null) return [];

                var items = new List<BookmarkItem>();
                CollectBookmarks(roots, items);
                return items.OrderBy(b => b.Title, StringComparer.OrdinalIgnoreCase).ToList();
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

    private static void CollectBookmarks(JsonNode node, List<BookmarkItem> items, string folderPath = "")
    {
        switch (node)
        {
            case JsonObject obj:
            {
                var type = obj["type"]?.GetValue<string>();
                var name = obj["name"]?.GetValue<string>() ?? "";

                if (type == "url")
                {
                    var url = obj["url"]?.GetValue<string>() ?? "";
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        items.Add(new BookmarkItem
                        {
                            Title = string.IsNullOrWhiteSpace(name) ? url : name,
                            Url = url,
                            Folder = folderPath
                        });
                    }
                }
                else if (type == "folder")
                {
                    var nextFolder = string.IsNullOrWhiteSpace(folderPath) ? name : $"{folderPath} / {name}";
                    if (obj["children"] is JsonArray children)
                    {
                        foreach (var child in children)
                        {
                            if (child != null)
                                CollectBookmarks(child, items, nextFolder);
                        }
                    }
                }
                else if (obj["roots"] != null)
                {
                    CollectBookmarks(obj["roots"]!, items, folderPath);
                }
                else
                {
                    foreach (var prop in obj)
                    {
                        if (prop.Value != null)
                            CollectBookmarks(prop.Value, items, folderPath);
                    }
                }
                break;
            }
            case JsonArray array:
                foreach (var item in array)
                {
                    if (item != null)
                        CollectBookmarks(item, items, folderPath);
                }
                break;
        }
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { /* ignore */ }
    }
}
