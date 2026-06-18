using System.Drawing;
using System.Windows.Forms;
using ChromeProfilApp.Models;
using ChromeProfilApp.Services;

namespace ChromeProfilApp.Helpers;

internal static class BackupDetailLayoutBuilder
{
    public static Panel CreateHeader(BackupPreviewSummary preview)
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 118,
            Padding = new Padding(16, 12, 16, 8),
            BackColor = Color.White
        };

        header.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(32, 33, 36),
            Text =
                $"Seçili profil: {preview.ProfileCount}  |  " +
                $"Tahmini boyut: {ChromeService.FormatSize(preview.TotalBackupBytes)}\r\n\r\n" +
                $"Kayıt yeri: {preview.EstimatedFullPath}\r\n\r\n" +
                "Yedeklenmeyecek kısımları 'Yedek Ayarları' sekmesinden kapatın."
        });

        return header;
    }

    public static TabControl CreateTabs(BackupPreviewSummary preview, Label includedLabel, Label excludedLabel,
        CheckBox chkPasswords, CheckBox chkHistory, CheckBox chkCookies,
        CheckBox chkExtensions, CheckBox chkCache)
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
        tabs.TabPages.Add(CreateProfilesTab(preview));
        tabs.TabPages.Add(CreateOptionsTab(includedLabel, excludedLabel, chkPasswords, chkHistory, chkCookies, chkExtensions, chkCache));
        tabs.TabPages.Add(CreateContentTab());
        return tabs;
    }

    public static Panel CreateFooter(bool confirmMode, Action onStart, Action onSecondary)
    {
        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 62,
            Padding = new Padding(16, 8, 16, 8),
            BackColor = Color.White
        };
        var btnStart = CreateButton("Yedeklemeyi Başlat", Color.FromArgb(26, 115, 232), onStart);
        var btnSecondary = CreateButton(confirmMode ? "İptal" : "Kapat",
            Color.FromArgb(95, 99, 104), onSecondary);

        // Make buttons auto-size and avoid clipping on different DPI settings.
        btnStart.AutoSize = true;
        btnStart.AutoSizeMode = AutoSizeMode.GrowOnly;
        btnStart.MinimumSize = new Size(160, 38);
        btnSecondary.AutoSize = true;
        btnSecondary.AutoSizeMode = AutoSizeMode.GrowOnly;
        btnSecondary.MinimumSize = new Size(120, 38);

        // Use a right-aligned FlowLayoutPanel so buttons remain visible on smaller widths
        var rightFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 8, 8),
            Margin = new Padding(0),
            WrapContents = false,
            AutoSize = false
        };

        rightFlow.Controls.Add(btnSecondary);
        rightFlow.Controls.Add(btnStart);

        footer.Controls.Add(rightFlow);
        return footer;
    }

    private static TabPage CreateProfilesTab(BackupPreviewSummary preview)
    {
        var page = new TabPage("Seçili Profiller") { Padding = new Padding(8) };
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

        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Profil", FillWeight = 14 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Klasör", FillWeight = 10 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Hesaplar", FillWeight = 22 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Yedek Boyutu", FillWeight = 11 });

        foreach (var p in preview.Profiles)
        {
            grid.Rows.Add(
                p.Name,
                p.Folder,
                p.Accounts.Count > 0 ? p.EmailDisplay : p.Email,
                ChromeService.FormatSize(p.BackupSizeBytes));
        }

        page.Controls.Add(grid);
        return page;
    }

    private static TabPage CreateOptionsTab(Label includedLabel, Label excludedLabel,
        CheckBox chkPasswords, CheckBox chkHistory, CheckBox chkCookies,
        CheckBox chkExtensions, CheckBox chkCache)
    {
        var page = new TabPage("Yedek Ayarları") { Padding = new Padding(16), BackColor = Color.White };

        var info = new Label
        {
            Dock = DockStyle.Top,
            Height = 40,
            ForeColor = Color.FromArgb(95, 99, 104),
            Text = "İşaretini kaldırdığınız kısımlar yedeklenmez (kopyalandıktan sonra silinir)."
        };

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 160,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = false
        };
        panel.Controls.AddRange(new Control[] { chkPasswords, chkHistory, chkCookies, chkExtensions, chkCache });

        includedLabel.Dock = DockStyle.Top;
        includedLabel.Height = 80;
        includedLabel.ForeColor = Color.FromArgb(52, 168, 83);

        excludedLabel.Dock = DockStyle.Fill;
        excludedLabel.ForeColor = Color.FromArgb(217, 48, 37);

        page.Controls.Add(excludedLabel);
        page.Controls.Add(includedLabel);
        page.Controls.Add(panel);
        page.Controls.Add(info);
        return page;
    }

    private static TabPage CreateContentTab()
    {
        var page = new TabPage("Bilgi") { Padding = new Padding(12), BackColor = Color.White };
        page.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(32, 33, 36),
            Text =
                "Her zaman yedeklenir:\r\n" +
                "• Yer imleri (Bookmarks)\r\n" +
                "• Profil ayarları (Preferences)\r\n" +
                "• Local State (profil listesi)\r\n\r\n" +
                "Profil seçimi ana ekrandaki 'Yedekle' sütunundan yapılır.\r\n\r\n" +
                "Geri yüklemede yedekte olmayan kısımlar (ör. şifre hariç yedek) geri gelmez."
        });
        return page;
    }

    private static Button CreateButton(string text, Color backColor, Action onClick)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(130, 34),
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += (_, _) => onClick();
        return btn;
    }
}
