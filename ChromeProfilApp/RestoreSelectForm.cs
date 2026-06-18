using System.Text.Json;
using ChromeProfilApp.Helpers;
using ChromeProfilApp.Models;
using ChromeProfilApp.Services;

namespace ChromeProfilApp;

public sealed class RestoreSelectForm : Form
{
    private readonly CheckedListBox _list;
    private readonly BackupManifest _manifest;
    public List<string> SelectedFolders { get; private set; } = [];

    public RestoreSelectForm(string backupPath)
    {
        _manifest = JsonSerializer.Deserialize<BackupManifest>(
            File.ReadAllText(Path.Combine(backupPath, "manifest.json")))
            ?? throw new InvalidOperationException("manifest okunamadı");

        Text = "Geri Yüklenecek Profiller";
        AppIconHelper.Apply(this);
        Size = new Size(520, 420);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10F);

        var info = RestoreSelectLayoutBuilder.CreateInfoLabel(backupPath);
        _list = RestoreSelectLayoutBuilder.CreateProfileList(_manifest);
        var btnPanel = RestoreSelectLayoutBuilder.CreateButtonPanel(
            () => SetAllProfileSelection(true),
            () => SetAllProfileSelection(false),
            () => { });

        Controls.Add(_list);
        Controls.Add(btnPanel);
        Controls.Add(info);

        AcceptButton = btnPanel.Controls.OfType<Button>().First(b => b.DialogResult == DialogResult.OK);

        FormClosing += (_, e) =>
        {
            if (DialogResult != DialogResult.OK) return;
            SelectedFolders = [];
            for (var i = 0; i < _list.CheckedIndices.Count; i++)
            {
                var idx = _list.CheckedIndices[i];
                if (idx < _manifest.Profiles.Count)
                    SelectedFolders.Add(_manifest.Profiles[idx].folder);
            }
            if (SelectedFolders.Count == 0)
            {
                MessageBox.Show("En az bir profil seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }
        };
    }

    private void SetAllProfileSelection(bool select)
    {
        for (var i = 0; i < _list.Items.Count; i++)
            _list.SetItemChecked(i, select);
    }
}
