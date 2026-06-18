using System.Text.Json;
using ChromeProfilApp.Helpers;
using ChromeProfilApp.Models;
using ChromeProfilApp.Services;

namespace ChromeProfilApp;

public sealed class BackupCompareForm : Form
{
    public BackupCompareForm(IReadOnlyList<BackupInfo> backups)
    {
        Text = "Yedek Karşılaştırma";
        AppIconHelper.Apply(this);
        Size = new Size(900, 520);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10F);

        var top = BackupCompareFormLayoutBuilder.CreateTopPanel(backups, out var combo1, out var combo2, out var btnCompare);
        var grid = BackupCompareFormLayoutBuilder.CreateGrid();

        btnCompare.Click += (_, _) =>
        {
            if (combo1.SelectedItem is not BackupCompareFormLayoutBuilder.BackupItemViewModel a ||
                combo2.SelectedItem is not BackupCompareFormLayoutBuilder.BackupItemViewModel b) return;
            Compare(grid, a.Info.FolderPath, b.Info.FolderPath);
        };

        Controls.Add(grid);
        Controls.Add(top);

        if (combo1.Items.Count > 0 && combo2.Items.Count > 0)
            Compare(grid,
                ((BackupCompareFormLayoutBuilder.BackupItemViewModel)combo1.Items[0]!).Info.FolderPath,
                ((BackupCompareFormLayoutBuilder.BackupItemViewModel)combo2.Items[Math.Min(1, combo2.Items.Count - 1)]!).Info.FolderPath);
    }

    private static void Compare(DataGridView grid, string path1, string path2)
    {
        var m1 = LoadManifest(path1);
        var m2 = LoadManifest(path2);
        grid.Rows.Clear();

        var allFolders = m1.Profiles.Select(p => p.folder)
            .Union(m2.Profiles.Select(p => p.folder))
            .Distinct()
            .OrderBy(f => f);

        foreach (var folder in allFolders)
        {
            var p1 = m1.Profiles.FirstOrDefault(p => p.folder == folder);
            var p2 = m2.Profiles.FirstOrDefault(p => p.folder == folder);
            var status = p1 == null ? "Sadece Yedek 2'de"
                : p2 == null ? "Sadece Yedek 1'de"
                : p1.email == p2.email ? "Her ikisinde" : "Farklı e-posta";
            grid.Rows.Add(
                p1?.name ?? p2?.name ?? folder,
                p1 != null ? $"{p1.name} ({p1.email})" : "—",
                p2 != null ? $"{p2.name} ({p2.email})" : "—",
                status);
        }

        grid.Rows.Add("---", $"Profil: {m1.ProfileCount}", $"Profil: {m2.ProfileCount}", "");
        grid.Rows.Add("---", m1.Created.ToString("dd.MM.yyyy HH:mm"), m2.Created.ToString("dd.MM.yyyy HH:mm"), "");
        grid.Rows.Add("---", m1.Computer, m2.Computer, "");
    }

    private static BackupManifest LoadManifest(string path)
    {
        return JsonSerializer.Deserialize<BackupManifest>(
            File.ReadAllText(Path.Combine(path, "manifest.json"))) ?? new BackupManifest();
    }
}
