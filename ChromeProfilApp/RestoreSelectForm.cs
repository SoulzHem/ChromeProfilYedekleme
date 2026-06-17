using System.Text.Json;
using ChromeProfilApp.Models;
using ChromeProfilApp.Services;

namespace ChromeProfilApp;

public sealed class RestoreSelectForm : Form
{
    private readonly CheckedListBox _list = new();
    public List<string> SelectedFolders { get; private set; } = [];

    public RestoreSelectForm(string backupPath)
    {
        Text = "Geri Yüklenecek Profiller";
        Size = new Size(520, 420);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10F);

        var manifest = JsonSerializer.Deserialize<BackupManifest>(
            File.ReadAllText(Path.Combine(backupPath, "manifest.json")))
            ?? throw new InvalidOperationException("manifest okunamadı");

        var info = new Label
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(12, 8, 12, 0),
            Text = $"Yedek: {Path.GetFileName(backupPath)}\nHangi profiller geri yüklensin?"
        };

        _list.Dock = DockStyle.Fill;
        _list.CheckOnClick = true;
        _list.Font = new Font("Segoe UI", 10F);
        foreach (var p in manifest.Profiles)
        {
            _list.Items.Add($"{p.name}  ({p.folder})  —  {p.email}", true);
        }

        var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(12, 8, 12, 8) };
        var btnOk = new Button
        {
            Text = "Geri Yükle",
            DialogResult = DialogResult.OK,
            Location = new Point(260, 8),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(52, 168, 83),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnOk.FlatAppearance.BorderSize = 0;
        var btnAll = new Button { Text = "Tümü", Location = new Point(12, 8), Size = new Size(70, 30) };
        var btnNone = new Button { Text = "Hiçbiri", Location = new Point(90, 8), Size = new Size(70, 30) };
        btnAll.Click += (_, _) => { for (var i = 0; i < _list.Items.Count; i++) _list.SetItemChecked(i, true); };
        btnNone.Click += (_, _) => { for (var i = 0; i < _list.Items.Count; i++) _list.SetItemChecked(i, false); };
        btnPanel.Controls.AddRange([btnAll, btnNone, btnOk]);

        Controls.Add(_list);
        Controls.Add(btnPanel);
        Controls.Add(info);
        AcceptButton = btnOk;

        FormClosing += (_, e) =>
        {
            if (DialogResult != DialogResult.OK) return;
            SelectedFolders = [];
            for (var i = 0; i < _list.CheckedIndices.Count; i++)
            {
                var idx = _list.CheckedIndices[i];
                if (idx < manifest.Profiles.Count)
                    SelectedFolders.Add(manifest.Profiles[idx].folder);
            }
            if (SelectedFolders.Count == 0)
            {
                MessageBox.Show("En az bir profil seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }
        };
    }
}
