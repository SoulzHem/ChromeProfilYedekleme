using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChromeProfilApp.Helpers;

internal static class MainFormLayoutBuilder
{
    public static Panel CreateHeader(out PictureBox headerIcon)
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 108,
            BackColor = Color.FromArgb(26, 115, 232)
        };

        header.Paint += (_, e) =>
        {
            using var brush = new LinearGradientBrush(header.ClientRectangle,
                Color.FromArgb(26, 115, 232), Color.FromArgb(66, 133, 244), 0f);
            e.Graphics.FillRectangle(brush, header.ClientRectangle);
        };

        header.Controls.Add(new Label
        {
            Text = "Chrome Profil Yedekleme",
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(72, 16)
        });
        header.Controls.Add(new Label
        {
            Text = "Profillerinizi yedekleyin, format sonrası tek tıkla geri yükleyin",
            ForeColor = Color.FromArgb(230, 240, 255),
            Font = new Font("Segoe UI", 9.5F),
            AutoSize = true,
            Location = new Point(74, 58)
        });

        headerIcon = new PictureBox
        {
            Size = new Size(40, 40),
            Location = new Point(24, 22),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        header.Controls.Add(headerIcon);
        return header;
    }

    public static TableLayoutPanel CreateBackupPathPanel(TextBox backupPathBox, Button browseBackupButton)
    {
        var pathPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            Padding = new Padding(12, 10, 12, 6),
            BackColor = Color.White,
            ColumnCount = 3
        };
        pathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
        pathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        pathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

        pathPanel.Controls.Add(new Label
        {
            Text = "Yedek klasörü:",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(60, 64, 67)
        }, 0, 0);
        pathPanel.Controls.Add(backupPathBox, 1, 0);
        pathPanel.Controls.Add(browseBackupButton, 2, 0);

        return pathPanel;
    }

    public static Panel CreateToolbar(Button refresh, Button backupDetail, Button backup, Button restore, Button detail, Button openBackupFolder)
    {
        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            Padding = new Padding(16, 8, 16, 8),
            BackColor = Color.White
        };

        toolbar.Controls.AddRange(new Control[] { refresh, backupDetail, backup, restore, detail, openBackupFolder });
        return toolbar;
    }

    public static Panel CreateSecondaryToolbar(Button compare, Button schedule)
    {
        var toolbar2 = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(16, 4, 16, 8),
            BackColor = Color.White
        };

        toolbar2.Controls.AddRange(new Control[] { compare, schedule });
        return toolbar2;
    }

    public static Panel CreateProfilePanel(DataGridView grid, Panel selectPanel, Label profileCountLabel)
    {
        var profilePanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 8, 16, 8),
            BackColor = Color.FromArgb(248, 249, 252)
        };

        profilePanel.Controls.Add(grid);
        profilePanel.Controls.Add(selectPanel);
        profilePanel.Controls.Add(profileCountLabel);
        return profilePanel;
    }

    public static Panel CreateBottomPanel(ComboBox backupCombo, ProgressBar progress, Label statusLabel, TextBox logBox)
    {
        var bottomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 190,
            Padding = new Padding(16, 8, 16, 16),
            BackColor = Color.FromArgb(248, 249, 252)
        };

        bottomPanel.Controls.Add(new Label
        {
            Text = "Geri yüklenecek yedek:",
            AutoSize = true,
            Location = new Point(16, 8),
            ForeColor = Color.FromArgb(60, 64, 67)
        });

        bottomPanel.Controls.Add(backupCombo);
        bottomPanel.Controls.Add(progress);
        bottomPanel.Controls.Add(statusLabel);
        bottomPanel.Controls.Add(logBox);
        return bottomPanel;
    }
}
