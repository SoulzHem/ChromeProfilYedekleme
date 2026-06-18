using ChromeProfilApp.Models;

namespace ChromeProfilApp.Helpers;

public static class BackupCompareFormLayoutBuilder
{
    public static Panel CreateTopPanel(IReadOnlyList<BackupInfo> backups, out ComboBox combo1, out ComboBox combo2, out Button btnCompare)
    {
        var top = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(12) };

        combo1 = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(12, 12), Size = new Size(380, 28) };
        combo2 = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(12, 44), Size = new Size(380, 28) };

        foreach (var b in backups)
        {
            var label = $"{b.FolderName} — {b.Created:dd.MM.yyyy HH:mm}";
            combo1.Items.Add(new BackupItemViewModel(b, label));
            combo2.Items.Add(new BackupItemViewModel(b, label));
        }

        combo1.DisplayMember = "Label";
        combo2.DisplayMember = "Label";
        if (combo1.Items.Count > 0) combo1.SelectedIndex = 0;
        if (combo2.Items.Count > 1) combo2.SelectedIndex = 1;

        btnCompare = new Button
        {
            Text = "Karşılaştır",
            Location = new Point(410, 28),
            Size = new Size(110, 30),
            BackColor = Color.FromArgb(26, 115, 232),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnCompare.FlatAppearance.BorderSize = 0;

        top.Controls.AddRange(new Control[] { combo1, combo2, btnCompare });
        return top;
    }

    public static DataGridView CreateGrid()
    {
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

        return grid;
    }

    public sealed class BackupItemViewModel(BackupInfo info, string label)
    {
        public BackupInfo Info { get; } = info;
        public string Label { get; } = label;
        public override string ToString() => Label;
    }
}
