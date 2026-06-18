using System.Windows.Forms;
using ChromeProfilApp.Models;

namespace ChromeProfilApp.Helpers;

internal static class RestoreSelectLayoutBuilder
{
    public static Label CreateInfoLabel(string backupPath)
    {
        return new Label
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(12, 8, 12, 0),
            Text = $"Yedek: {Path.GetFileName(backupPath)}\nHangi profiller geri yüklensin?"
        };
    }

    public static CheckedListBox CreateProfileList(BackupManifest manifest)
    {
        var list = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            CheckOnClick = true,
            Font = new Font("Segoe UI", 10F)
        };

        foreach (var p in manifest.Profiles)
        {
            list.Items.Add($"{p.name}  ({p.folder})  —  {p.email}", true);
        }

        return list;
    }

    public static Panel CreateButtonPanel(Action onSelectAll, Action onSelectNone, Action onRestore)
    {
        var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(12, 8, 12, 8) };

        var btnAll = new Button
        {
            Text = "Tümü",
            Location = new Point(12, 8),
            Size = new Size(70, 30),
            FlatStyle = FlatStyle.Flat
        };
        btnAll.Click += (_, _) => onSelectAll();

        var btnNone = new Button
        {
            Text = "Hiçbiri",
            Location = new Point(90, 8),
            Size = new Size(70, 30),
            FlatStyle = FlatStyle.Flat
        };
        btnNone.Click += (_, _) => onSelectNone();

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
        btnOk.Click += (_, _) => onRestore();

        btnPanel.Controls.AddRange(new Control[] { btnAll, btnNone, btnOk });
        return btnPanel;
    }
}
