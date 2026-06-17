using ChromeProfilApp.Helpers;
using ChromeProfilApp.Models;
using ChromeProfilApp.Services;

namespace ChromeProfilApp;

public sealed class ProfileDetailForm : Form
{
    private readonly ChromeService _service;
    private readonly ChromeProfile _profile;
    private readonly DataGridView _passwordGrid = new();
    private readonly DataGridView _bookmarkGrid = new();
    private readonly DataGridView _accountGrid = new();
    private readonly DataGridView _extensionGrid = new();
    private readonly DataGridView _historyGrid = new();
    private readonly CheckBox _showPasswords = new();
    private readonly Label _pwdInfoLabel = new();
    private List<SavedPassword> _passwords = [];
    private List<BookmarkItem> _bookmarks = [];
    private List<HistoryItem> _history = [];
    private ProfileStats _stats = new();

    public ProfileDetailForm(ChromeService service, ChromeProfile profile)
    {
        _service = service;
        _profile = profile;

        Text = $"Profil Detayı — {profile.Name}";
        AppIconHelper.Apply(this);
        Size = new Size(1000, 700);
        MinimumSize = new Size(880, 580);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10F);
        BackColor = Color.FromArgb(248, 249, 252);
        Cursor = Cursors.Default;

        BuildLayout();
        Load += async (_, _) => await LoadDataAsync();
    }

    private void BuildLayout()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };

        tabs.TabPages.Add(BuildSummaryTab());
        tabs.TabPages.Add(BuildAccountsTab());
        tabs.TabPages.Add(BuildExtensionsTab());
        tabs.TabPages.Add(BuildBookmarksTab());
        tabs.TabPages.Add(BuildHistoryTab());
        tabs.TabPages.Add(BuildPasswordsTab());

        var exportBar = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.White, Padding = new Padding(12, 8, 12, 8) };
        var lblExport = new Label { Text = "CSV dışa aktar:", AutoSize = true, Location = new Point(12, 12) };
        var btnBookmarks = CreateExportBtn("Yer İmleri", () => ExportBookmarks());
        var btnPasswords = CreateExportBtn("Şifreler", () => ExportPasswords());
        var btnAccounts = CreateExportBtn("E-postalar", () => ExportAccounts());
        var btnHistory = CreateExportBtn("Geçmiş", () => ExportHistory());
        btnBookmarks.Location = new Point(120, 6);
        btnPasswords.Location = new Point(220, 6);
        btnAccounts.Location = new Point(310, 6);
        btnHistory.Location = new Point(410, 6);
        exportBar.Controls.AddRange([lblExport, btnBookmarks, btnPasswords, btnAccounts, btnHistory]);

        Controls.Add(tabs);
        Controls.Add(exportBar);
    }

    private static Button CreateExportBtn(string text, Action onClick)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(90, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(248, 249, 252),
            ForeColor = Color.FromArgb(26, 115, 232),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = Color.FromArgb(26, 115, 232);
        btn.Click += (_, _) => onClick();
        return btn;
    }

    private void ExportBookmarks() => ExportCsv("yer_imleri", path => ExportService.ExportBookmarksToCsv(_bookmarks, path));
    private void ExportPasswords() => ExportCsv("sifreler", path => ExportService.ExportPasswordsToCsv(_passwords, path));
    private void ExportAccounts() => ExportCsv("epostalar", path => ExportService.ExportAccountsToCsv(_profile.Accounts, path));
    private void ExportHistory() => ExportCsv("gecmis", path => ExportService.ExportHistoryToCsv(_history, path));

    private void ExportCsv(string prefix, Action<string> export)
    {
        using var dlg = new SaveFileDialog
        {
            Filter = "CSV dosyası|*.csv",
            FileName = $"{prefix}_{_profile.Folder}_{DateTime.Now:yyyyMMdd}.csv"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        try
        {
            export(dlg.FileName);
            MessageBox.Show($"Kaydedildi:\n{dlg.FileName}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private TabPage BuildHistoryTab()
    {
        var page = new TabPage("Geçmiş") { Padding = new Padding(8) };
        ConfigureGrid(_historyGrid);
        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Başlık", FillWeight = 25 });
        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Adres", FillWeight = 40 });
        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ziyaret", FillWeight = 10 });
        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Son", FillWeight = 15 });
        _historyGrid.Dock = DockStyle.Fill;
        page.Controls.Add(_historyGrid);
        return page;
    }

    private TabPage BuildSummaryTab()
    {
        var page = new TabPage("Özet") { BackColor = Color.White, Padding = new Padding(12) };

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(8),
            BackColor = Color.White
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _summaryTable = table;
        scroll.Controls.Add(table);
        page.Controls.Add(scroll);
        return page;
    }

    private TableLayoutPanel _summaryTable = null!;

    private void FillSummaryTab()
    {
        _summaryTable.Controls.Clear();
        _summaryTable.RowStyles.Clear();
        _summaryTable.RowCount = 0;

        var cacheSize = _profile.DiskSizeBytes - _profile.BackupSizeBytes;
        var lastActive = _profile.LastActive?.ToString("dd.MM.yyyy HH:mm") ?? "-";
        var googleCount = _profile.Accounts.Count(a => a.AccountKind.StartsWith("Google", StringComparison.OrdinalIgnoreCase));
        var siteCount = _profile.Accounts.Count(a => a.AccountKind == "Site Girişi");

        AddRow(_summaryTable, "Profil adı", _profile.Name);
        AddRow(_summaryTable, "Ana e-posta", _profile.Email);
        AddRow(_summaryTable, "Google hesapları", googleCount.ToString());
        AddRow(_summaryTable, "Site giriş e-postaları", siteCount.ToString());
        AddRow(_summaryTable, "Klasör", _profile.Folder);
        AddRow(_summaryTable, "Profil yolu", _stats.ProfilePath);
        AddRow(_summaryTable, "Chrome sürümü", _stats.ChromeVersion);
        AddRow(_summaryTable, "Geçmiş kaydı", _stats.HistoryCount.ToString("N0"));
        AddRow(_summaryTable, "Oturum dosyası", _stats.OpenTabCount > 0 ? "Var (açık sekmeler kayıtlı)" : "Yok");
        AddRow(_summaryTable, "Yer imi", _profile.BookmarkCount.ToString());
        AddRow(_summaryTable, "Kayıtlı şifre", _profile.PasswordCount.ToString());
        AddRow(_summaryTable, "Eklenti", _profile.ExtensionCount.ToString());
        AddRow(_summaryTable, "Son kullanım", lastActive);
        AddRow(_summaryTable, "Yedek boyutu", ChromeService.FormatSize(_profile.BackupSizeBytes));
        AddRow(_summaryTable, "Disk boyutu", $"{ChromeService.FormatSize(_profile.DiskSizeBytes)} (önbellek dahil)");
        AddRow(_summaryTable, "Önbellek", $"~{ChromeService.FormatSize(Math.Max(0, cacheSize))} (yedeklenmez)");
    }

    private TabPage BuildAccountsTab()
    {
        var page = new TabPage("E-postalar") { Padding = new Padding(8) };

        var info = new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            ForeColor = Color.FromArgb(95, 99, 104),
            Text = "Google Hesabı = Chrome'a giriş yaptığınız hesap. Site Girişi = sitelere kaydettiğiniz e-posta adresleri."
        };

        ConfigureGrid(_accountGrid);
        _accountGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "E-posta", FillWeight = 32 });
        _accountGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ad", FillWeight = 22 });
        _accountGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tür", FillWeight = 18 });
        _accountGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Not", FillWeight = 14 });
        _accountGrid.Dock = DockStyle.Fill;

        page.Controls.Add(_accountGrid);
        page.Controls.Add(info);
        return page;
    }

    private TabPage BuildExtensionsTab()
    {
        var page = new TabPage("Eklentiler") { Padding = new Padding(8) };
        ConfigureGrid(_extensionGrid);
        _extensionGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Eklenti", FillWeight = 40 });
        _extensionGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sürüm", FillWeight = 15 });
        _extensionGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kimlik", FillWeight = 30 });
        _extensionGrid.Dock = DockStyle.Fill;
        page.Controls.Add(_extensionGrid);
        return page;
    }

    private TabPage BuildBookmarksTab()
    {
        var page = new TabPage($"Yer İmleri ({_profile.BookmarkCount})") { Padding = new Padding(8) };
        ConfigureGrid(_bookmarkGrid);
        _bookmarkGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Başlık", FillWeight = 28 });
        _bookmarkGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Klasör", FillWeight = 22 });
        _bookmarkGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Adres", FillWeight = 50 });
        _bookmarkGrid.Dock = DockStyle.Fill;
        page.Controls.Add(_bookmarkGrid);
        return page;
    }

    private TabPage BuildPasswordsTab()
    {
        var page = new TabPage($"Şifreler ({_profile.PasswordCount})") { Padding = new Padding(8) };

        var top = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.FromArgb(248, 249, 252) };

        _pwdInfoLabel.Dock = DockStyle.Top;
        _pwdInfoLabel.Height = 36;
        _pwdInfoLabel.ForeColor = Color.FromArgb(95, 99, 104);
        _pwdInfoLabel.Text = "Chrome kapalı olmalı. Yeni Chrome sürümlerinde bazı şifreler v20 koruması nedeniyle görüntülenemeyebilir.";

        var btnRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 36,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        _showPasswords.Text = "Şifreleri göster";
        _showPasswords.AutoSize = true;
        _showPasswords.Margin = new Padding(0, 6, 16, 0);
        _showPasswords.CheckedChanged += (_, _) => RefreshPasswordGrid();

        var btnReload = new Button
        {
            Text = "Yeniden yükle",
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(26, 115, 232),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnReload.FlatAppearance.BorderSize = 0;
        btnReload.Click += async (_, _) => await LoadDataAsync();

        btnRow.Controls.Add(_showPasswords);
        btnRow.Controls.Add(btnReload);

        top.Controls.Add(btnRow);
        top.Controls.Add(_pwdInfoLabel);

        ConfigureGrid(_passwordGrid);
        _passwordGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Site", FillWeight = 35 });
        _passwordGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kullanıcı", FillWeight = 25 });
        _passwordGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Şifre", FillWeight = 25 });
        _passwordGrid.Dock = DockStyle.Fill;

        page.Controls.Add(_passwordGrid);
        page.Controls.Add(top);
        return page;
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowTemplate.Height = 30;
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
    }

    private static void AddRow(TableLayoutPanel table, string label, string value)
    {
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        table.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = Color.FromArgb(95, 99, 104),
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            Margin = new Padding(0, 8, 8, 8)
        }, 0, row);

        table.Controls.Add(new Label
        {
            Text = value,
            AutoSize = true,
            MaximumSize = new Size(740, 0),
            Anchor = AnchorStyles.Left,
            ForeColor = Color.FromArgb(32, 33, 36),
            Margin = new Padding(0, 8, 0, 8)
        }, 1, row);
    }

    private async Task LoadDataAsync()
    {
        UseWaitCursor = true;
        try
        {
            _profile.Accounts = await Task.Run(() => _service.GetProfileAccounts(_profile));
            _stats = await Task.Run(() => _service.GetProfileStats(_profile));
            FillSummaryTab();

            var tab = TabControlFromAccountGrid();
            if (tab != null)
                tab.Text = $"E-postalar ({_profile.Accounts.Count})";

            _accountGrid.Rows.Clear();
            if (_profile.Accounts.Count == 0)
            {
                _accountGrid.Rows.Add(_profile.Email, _profile.Name, "Google Hesabı", "Ana");
            }
            else
            {
                foreach (var a in _profile.Accounts)
                {
                    _accountGrid.Rows.Add(
                        a.Email,
                        string.IsNullOrWhiteSpace(a.FullName) ? "-" : a.FullName,
                        a.AccountKind,
                        a.IsPrimary ? "Ana" : "-");
                }
            }

            _extensionGrid.Rows.Clear();
            var extensions = await Task.Run(() => _service.GetProfileExtensions(_profile));
            var extTab = TabControlFromExtensionGrid();
            if (extTab != null)
                extTab.Text = $"Eklentiler ({extensions.Count})";

            if (extensions.Count == 0)
                _extensionGrid.Rows.Add("(Eklenti yok)", "", "");
            else
                foreach (var e in extensions)
                    _extensionGrid.Rows.Add(e.Name, e.Version, e.Id);

            _passwordGrid.Rows.Clear();
            _passwordGrid.Rows.Add("Yükleniyor...", "", "");
            _bookmarkGrid.Rows.Clear();
            _bookmarkGrid.Rows.Add("Yükleniyor...", "", "");

            if (_service.IsChromeRunning())
            {
                _pwdInfoLabel.ForeColor = Color.FromArgb(217, 48, 37);
                _pwdInfoLabel.Text = "Chrome açık! Şifreler için Chrome'u kapatın ve 'Yeniden yükle'ye tıklayın.";
            }
            else
            {
                _pwdInfoLabel.ForeColor = Color.FromArgb(95, 99, 104);
                _pwdInfoLabel.Text = "Eski şifreler (v10) görüntülenebilir. Yeni Chrome v20 şifreleri ek koruma altında olabilir.";
            }

            var bookmarks = await Task.Run(() => _service.GetProfileBookmarks(_profile));
            _bookmarks = bookmarks;
            _bookmarkGrid.Rows.Clear();
            if (bookmarks.Count == 0)
                _bookmarkGrid.Rows.Add("(Yer imi bulunamadı)", "", "");
            else
                foreach (var b in bookmarks)
                    _bookmarkGrid.Rows.Add(b.Title, b.Folder, b.Url);

            _passwords = await Task.Run(() => _service.GetProfilePasswords(_profile));
            RefreshPasswordGrid();

            _history = await Task.Run(() => _service.GetProfileHistory(_profile));
            _historyGrid.Rows.Clear();
            if (_history.Count == 0)
                _historyGrid.Rows.Add("(Geçmiş bulunamadı)", "", "", "");
            else
                foreach (var h in _history)
                    _historyGrid.Rows.Add(h.Title, h.Url, h.VisitCount, h.LastVisit?.ToString("dd.MM.yy HH:mm") ?? "-");
        }
        catch (Exception ex)
        {
            _passwordGrid.Rows.Clear();
            _passwordGrid.Rows.Add($"Hata: {ex.Message}", "", "");
        }
        finally
        {
            UseWaitCursor = false;
            Cursor = Cursors.Default;
        }
    }

    private TabPage? TabControlFromAccountGrid()
    {
        if (_accountGrid.Parent is TabPage tp) return tp;
        return null;
    }

    private TabPage? TabControlFromExtensionGrid()
    {
        if (_extensionGrid.Parent is TabPage tp) return tp;
        return null;
    }

    private void RefreshPasswordGrid()
    {
        _passwordGrid.Rows.Clear();

        if (_passwords.Count == 0)
        {
            _passwordGrid.Rows.Add("(Kayıtlı şifre bulunamadı veya okunamadı)", "", "");
            return;
        }

        foreach (var p in _passwords)
        {
            var isProtected = p.Password.StartsWith('(') && p.Password.EndsWith(')');
            var pwd = _showPasswords.Checked || isProtected
                ? p.Password
                : new string('•', Math.Max(8, Math.Min(p.Password.Length, 12)));
            _passwordGrid.Rows.Add(p.Site, p.Username, pwd);
        }
    }
}
