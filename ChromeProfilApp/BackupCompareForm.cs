using System.Text.Json;
using ChromeProfilApp.Models;
using ChromeProfilApp.Services;

namespace ChromeProfilApp;

public sealed class BackupCompareForm : Form
{
    public BackupCompareForm(IReadOnlyList<BackupInfo> backups)
    {
        Text = "Yedek Karşılaştırma";
        Size = new Size(900, 520);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10F);

        var top = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(12) };
        var combo1 = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(12, 12), Size = new Size(380, 28) };
        var combo2 = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(12, 44), Size = new Size(380, 28) };
        foreach (var b in backups)
        {
            var label = $"{b.FolderName} — {b.Created:dd.MM.yyyy HH:mm}";
            combo1.Items.Add(new BackupItem(b, label));
            combo2.Items.Add(new BackupItem(b, label));
        }
        combo1.DisplayMember = "Label";
        combo2.DisplayMember = "Label";
        if (combo1.Items.Count > 0) combo1.SelectedIndex = 0;
        if (combo2.Items.Count > 1) combo2.SelectedIndex = 1;

        var btnCompare = new Button
        {
            Text = "Karşılaştır",
            Location = new Point(410, 28),
            Size = new Size(110, 30),
            BackColor = Color.FromArgb(26, 115, 232),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnCompare.FlatAppearance.BorderSize = 0;

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Profil", FillWeight = 20 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Yedek 1", FillWeight = 15 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Yedek 2", FillWeight = 15 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Durum", FillWeight = 15 });

        btnCompare.Click += (_, _) =>
        {
            if (combo1.SelectedItem is not BackupItem a || combo2.SelectedItem is not BackupItem b) return;
            Compare(grid, a.Info.FolderPath, b.Info.FolderPath);
        };

        top.Controls.AddRange([combo1, combo2, btnCompare]);
        Controls.Add(grid);
        Controls.Add(top);

        if (combo1.Items.Count > 0 && combo2.Items.Count > 0)
            Compare(grid, ((BackupItem)combo1.Items[0]!).Info.FolderPath,
                ((BackupItem)combo2.Items[Math.Min(1, combo2.Items.Count - 1)]!).Info.FolderPath);
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

    private sealed class BackupItem(BackupInfo info, string label)
    {
        public BackupInfo Info { get; } = info;
        public string Label { get; } = label;
        public override string ToString() => Label;
    }
}
