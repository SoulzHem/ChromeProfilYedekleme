using ChromeProfilApp.Helpers;
using ChromeProfilApp.Models;
using ChromeProfilApp.Services;

namespace ChromeProfilApp;

public sealed class BackupDetailForm : Form
{
    private readonly BackupPreviewSummary _preview;
    private readonly CheckBox _chkPasswords = new() { Text = "Şifreler (Login Data)", AutoSize = true, Checked = true };
    private readonly CheckBox _chkHistory = new() { Text = "Geçmiş (History, ziyaret edilen siteler)", AutoSize = true, Checked = true };
    private readonly CheckBox _chkCookies = new() { Text = "Çerezler ve oturum (Cookies, Network)", AutoSize = true, Checked = true };
    private readonly CheckBox _chkExtensions = new() { Text = "Eklentiler (Extensions klasörü)", AutoSize = true, Checked = true };
    private readonly CheckBox _chkCache = new() { Text = "Önbellek (Cache — boyutu artırır)", AutoSize = true, Checked = false };
    private readonly Label _includedLabel = new();
    private readonly Label _excludedLabel = new();

    public bool StartBackupConfirmed { get; private set; }
    public BackupOptions Options { get; private set; } = new();

    public BackupDetailForm(BackupPreviewSummary preview, bool confirmMode)
    {
        _preview = preview;
        Options = new BackupOptions
        {
            IncludePasswords = preview.Options.IncludePasswords,
            IncludeHistory = preview.Options.IncludeHistory,
            IncludeCookies = preview.Options.IncludeCookies,
            IncludeExtensions = preview.Options.IncludeExtensions,
            IncludeCache = preview.Options.IncludeCache
        };

        StartBackupConfirmed = false;
        Text = confirmMode ? "Yedekleme Onayı" : "Yedekleme Detayları";
        AppIconHelper.Apply(this);
        Size = new Size(980, 680);
        MinimumSize = new Size(860, 540);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10F);
        BackColor = Color.FromArgb(248, 249, 252);

        _chkPasswords.Checked = Options.IncludePasswords;
        _chkHistory.Checked = Options.IncludeHistory;
        _chkCookies.Checked = Options.IncludeCookies;
        _chkExtensions.Checked = Options.IncludeExtensions;
        _chkCache.Checked = Options.IncludeCache;

        foreach (var chk in new[] { _chkPasswords, _chkHistory, _chkCookies, _chkExtensions, _chkCache })
            chk.CheckedChanged += (_, _) => RefreshOptionSummary();

        BuildLayout(preview, confirmMode);
        RefreshOptionSummary();
    }

    private void BuildLayout(BackupPreviewSummary preview, bool confirmMode)
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

        var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
        tabs.TabPages.Add(BuildProfilesTab(preview));
        tabs.TabPages.Add(BuildOptionsTab());
        tabs.TabPages.Add(BuildContentTab());

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            Padding = new Padding(16, 8, 16, 8),
            BackColor = Color.White
        };

        if (confirmMode)
        {
            var btnStart = CreateButton("Yedeklemeyi Başlat", Color.FromArgb(26, 115, 232));
            btnStart.Location = new Point(680, 8);
            btnStart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStart.Click += (_, _) =>
            {
                Options = ReadOptions();
                StartBackupConfirmed = true;
                DialogResult = DialogResult.OK;
                Close();
            };

            var btnCancel = CreateButton("İptal", Color.FromArgb(95, 99, 104));
            btnCancel.Location = new Point(820, 8);
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

            footer.Controls.AddRange([btnStart, btnCancel]);
        }
        else
        {
            var btnClose = CreateButton("Kapat", Color.FromArgb(95, 99, 104));
            btnClose.Location = new Point(820, 8);
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click += (_, _) => Close();
            footer.Controls.Add(btnClose);
        }

        Controls.Add(tabs);
        Controls.Add(footer);
        Controls.Add(header);
    }

    private TabPage BuildOptionsTab()
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
        panel.Controls.AddRange([_chkPasswords, _chkHistory, _chkCookies, _chkExtensions, _chkCache]);

        _includedLabel.Dock = DockStyle.Top;
        _includedLabel.Height = 80;
        _includedLabel.ForeColor = Color.FromArgb(52, 168, 83);
        _excludedLabel.Dock = DockStyle.Fill;
        _excludedLabel.ForeColor = Color.FromArgb(217, 48, 37);

        page.Controls.Add(_excludedLabel);
        page.Controls.Add(_includedLabel);
        page.Controls.Add(panel);
        page.Controls.Add(info);
        return page;
    }

    private void RefreshOptionSummary()
    {
        var opt = ReadOptions();
        _includedLabel.Text = "Yedeklenecek:\r\n• " + string.Join("\r\n• ", opt.GetIncludedParts());
        var excluded = opt.GetExcludedParts();
        _excludedLabel.Text = excluded.Count == 0
            ? "Hariç tutulan: (yok — tam yedek)"
            : "Hariç tutulan:\r\n• " + string.Join("\r\n• ", excluded);
    }

    private BackupOptions ReadOptions() => new()
    {
        IncludePasswords = _chkPasswords.Checked,
        IncludeHistory = _chkHistory.Checked,
        IncludeCookies = _chkCookies.Checked,
        IncludeExtensions = _chkExtensions.Checked,
        IncludeCache = _chkCache.Checked
    };

    private static TabPage BuildProfilesTab(BackupPreviewSummary preview)
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

    private TabPage BuildContentTab()
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

    private static Button CreateButton(string text, Color backColor) => new()
    {
        Text = text,
        Size = new Size(130, 34),
        FlatStyle = FlatStyle.Flat,
        FlatAppearance = { BorderSize = 0 },
        BackColor = backColor,
        ForeColor = Color.White,
        Cursor = Cursors.Hand,
        Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
    };
}
