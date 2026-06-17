using System.Diagnostics;
using System.Drawing.Drawing2D;
using ChromeProfilApp.Helpers;
using ChromeProfilApp.Models;
using ChromeProfilApp.Services;

namespace ChromeProfilApp;

public sealed class MainForm : Form
{
    private readonly ChromeService _service = new();
    private readonly DataGridView _grid = new();
    private readonly ComboBox _backupCombo = new();
    private readonly ProgressBar _progress = new();
    private readonly TextBox _logBox = new();
    private readonly TextBox _backupPathBox = new();
    private readonly Label _statusLabel = new();
    private readonly Label _profileCountLabel = new();
    private readonly Button _btnRefresh = new();
    private readonly Button _btnBackup = new();
    private readonly Button _btnRestore = new();
    private readonly Button _btnOpenBackupFolder = new();
    private readonly Button _btnBrowseBackup = new();
    private readonly Button _btnDetail = new();
    private readonly Button _btnBackupDetail = new();
    private readonly Button _btnSelectAll = new();
    private readonly Button _btnSelectNone = new();
    private readonly Button _btnCompare = new();
    private readonly Button _btnSchedule = new();
    private List<ChromeProfile> _profiles = [];
    private bool _busy;
    private bool _loading;

    public MainForm()
    {
        Text = "Chrome Profil Yedekleme";
        Size = new Size(1080, 720);
        MinimumSize = new Size(920, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10F);
        BackColor = Color.FromArgb(248, 249, 252);
        Cursor = Cursors.Default;
        AppIconHelper.Apply(this);

        BuildLayout();
        Shown += async (_, _) => await LoadProfilesAsync();
    }

    private void BuildLayout()
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

        var headerIcon = new PictureBox
        {
            Size = new Size(40, 40),
            Location = new Point(24, 22),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        var appIcon = AppIconHelper.Load();
        if (appIcon != null)
            headerIcon.Image = appIcon.ToBitmap();
        header.Controls.Add(headerIcon);

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

        var pathLabel = new Label
        {
            Text = "Yedek klasörü:",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(60, 64, 67)
        };

        _backupPathBox.ReadOnly = true;
        _backupPathBox.Dock = DockStyle.Fill;
        _backupPathBox.BackColor = Color.FromArgb(248, 249, 252);
        _backupPathBox.BorderStyle = BorderStyle.FixedSingle;
        _backupPathBox.Text = _service.BackupRoot;

        StyleSmallButton(_btnBrowseBackup, "Klasör Seç", Color.FromArgb(26, 115, 232));
        _btnBrowseBackup.Dock = DockStyle.Fill;
        _btnBrowseBackup.Click += (_, _) => BrowseBackupFolder();

        pathPanel.Controls.Add(pathLabel, 0, 0);
        pathPanel.Controls.Add(_backupPathBox, 1, 0);
        pathPanel.Controls.Add(_btnBrowseBackup, 2, 0);

        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            Padding = new Padding(16, 8, 16, 8),
            BackColor = Color.White
        };

        StyleButton(_btnRefresh, "Yenile", Color.FromArgb(95, 99, 104));
        StyleButton(_btnBackupDetail, "Yedek Detayı", Color.FromArgb(66, 133, 244));
        StyleButton(_btnBackup, "Yedekle", Color.FromArgb(26, 115, 232));
        StyleButton(_btnRestore, "Geri Yükle", Color.FromArgb(52, 168, 83));
        StyleButton(_btnDetail, "Detay / Şifreler", Color.FromArgb(234, 134, 13));
        StyleButton(_btnOpenBackupFolder, "Klasörü Aç", Color.FromArgb(95, 99, 104));

        _btnRefresh.Location = new Point(16, 8);
        _btnBackupDetail.Location = new Point(130, 8);
        _btnBackupDetail.Size = new Size(118, 36);
        _btnBackup.Location = new Point(256, 8);
        _btnRestore.Location = new Point(370, 8);
        _btnDetail.Location = new Point(484, 8);
        _btnDetail.Size = new Size(130, 36);
        _btnOpenBackupFolder.Location = new Point(624, 8);

        _btnRefresh.Click += async (_, _) => await LoadProfilesAsync();
        _btnBackupDetail.Click += (_, _) => ShowBackupDetail(confirmMode: false);
        _btnBackup.Click += async (_, _) => await RunBackupAsync();
        _btnRestore.Click += async (_, _) => await RunRestoreAsync();
        _btnDetail.Click += (_, _) => OpenProfileDetail();
        _btnOpenBackupFolder.Click += (_, _) => OpenBackupFolder();

        toolbar.Controls.AddRange([_btnRefresh, _btnBackupDetail, _btnBackup, _btnRestore, _btnDetail, _btnOpenBackupFolder]);

        var toolbar2 = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(16, 4, 16, 8),
            BackColor = Color.White
        };
        StyleButton(_btnCompare, "Yedek Karşılaştır", Color.FromArgb(95, 99, 104));
        StyleButton(_btnSchedule, "Zamanlı Yedek", Color.FromArgb(95, 99, 104));
        _btnCompare.Size = new Size(140, 32);
        _btnSchedule.Size = new Size(120, 32);
        _btnCompare.Location = new Point(16, 6);
        _btnSchedule.Location = new Point(166, 6);
        _btnCompare.Click += (_, _) => OpenBackupCompare();
        _btnSchedule.Click += (_, _) => OpenScheduleBackup();
        toolbar2.Controls.AddRange([_btnCompare, _btnSchedule]);

        var profilePanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 8, 16, 8),
            BackColor = Color.FromArgb(248, 249, 252)
        };

        _profileCountLabel.Text = "Profiller yükleniyor...";
        _profileCountLabel.Dock = DockStyle.Top;
        _profileCountLabel.Height = 22;
        _profileCountLabel.Font = new Font("Segoe UI Semibold", 9.5F);
        _profileCountLabel.ForeColor = Color.FromArgb(32, 33, 36);

        var selectPanel = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = Color.FromArgb(248, 249, 252) };
        StyleLinkButton(_btnSelectAll, "Tümünü seç");
        StyleLinkButton(_btnSelectNone, "Seçimi kaldır");
        _btnSelectAll.Location = new Point(0, 4);
        _btnSelectNone.Location = new Point(110, 4);
        _btnSelectAll.Click += (_, _) => SetAllProfileSelection(true);
        _btnSelectNone.Click += (_, _) => SetAllProfileSelection(false);
        selectPanel.Controls.AddRange([_btnSelectAll, _btnSelectNone]);

        ConfigureGrid();
        _grid.Dock = DockStyle.Fill;
        _grid.CellDoubleClick += (_, e) =>
        {
            if (e.ColumnIndex != 0) OpenProfileDetail();
        };
        _grid.CellToolTipTextNeeded += GridCellToolTipTextNeeded;
        _grid.CellBeginEdit += (_, e) =>
        {
            if (_grid.Columns[e.ColumnIndex].Name != "Backup") e.Cancel = true;
        };
        _grid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_grid.IsCurrentCellDirty)
                _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        _grid.CellValueChanged += (_, e) =>
        {
            if (e.ColumnIndex >= 0 && _grid.Columns[e.ColumnIndex].Name == "Backup")
                UpdateProfileSummaryLabel();
        };

        profilePanel.Controls.Add(_grid);
        profilePanel.Controls.Add(selectPanel);
        profilePanel.Controls.Add(_profileCountLabel);

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

        _backupCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _backupCombo.Location = new Point(16, 32);
        _backupCombo.Size = new Size(620, 28);

        _progress.Location = new Point(16, 68);
        _progress.Size = new Size(1032, 22);
        _progress.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        _progress.Visible = false;

        _statusLabel.Text = "Hazır";
        _statusLabel.Location = new Point(16, 96);
        _statusLabel.AutoSize = true;
        _statusLabel.ForeColor = Color.FromArgb(26, 115, 232);

        _logBox.Multiline = true;
        _logBox.ReadOnly = true;
        _logBox.ScrollBars = ScrollBars.Vertical;
        _logBox.Location = new Point(16, 118);
        _logBox.Size = new Size(1032, 56);
        _logBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
        _logBox.BackColor = Color.White;
        _logBox.BorderStyle = BorderStyle.FixedSingle;
        _logBox.Font = new Font("Consolas", 9F);

        bottomPanel.Controls.AddRange([_backupCombo, _progress, _statusLabel, _logBox]);

        Controls.Add(profilePanel);
        Controls.Add(bottomPanel);
        Controls.Add(toolbar2);
        Controls.Add(toolbar);
        Controls.Add(pathPanel);
        Controls.Add(header);
    }

    private void ConfigureGrid()
    {
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        _grid.EnableHeadersVisualStyles = false;
        _grid.GridColor = Color.FromArgb(232, 234, 237);
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.ReadOnly = false;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowTemplate.Height = 34;
        _grid.ShowCellToolTips = true;
        _grid.EditMode = DataGridViewEditMode.EditOnEnter;

        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 244);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(60, 64, 67);
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        _grid.ColumnHeadersHeight = 38;

        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 240, 254);
        _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(32, 33, 36);
        _grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

        _grid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "Backup",
            HeaderText = "Yedekle",
            FillWeight = 7,
            TrueValue = true,
            FalseValue = false
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "No", HeaderText = "#", FillWeight = 5, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Profil", FillWeight = 14, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "E-posta / Hesaplar", FillWeight = 22, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Bookmarks", HeaderText = "Yer İmi", FillWeight = 8, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Passwords", HeaderText = "Şifre", FillWeight = 7, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Extensions", HeaderText = "Eklenti", FillWeight = 8, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "BackupSize", HeaderText = "Yedek Boyutu", FillWeight = 11, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DiskSize", HeaderText = "Disk (cache dahil)", FillWeight = 12, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "LastActive", HeaderText = "Son Kullanım", FillWeight = 12, ReadOnly = true });
    }

    private static void StyleLinkButton(Button btn, string text)
    {
        btn.Text = text;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = Color.FromArgb(248, 249, 252);
        btn.ForeColor = Color.FromArgb(26, 115, 232);
        btn.Cursor = Cursors.Hand;
        btn.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Underline);
        btn.Size = new Size(100, 24);
    }

    private void SetAllProfileSelection(bool selected)
    {
        foreach (DataGridViewRow row in _grid.Rows)
            row.Cells["Backup"].Value = selected;
        UpdateProfileSummaryLabel();
    }

    private List<ChromeProfile> GetSelectedProfiles()
    {
        var selected = new List<ChromeProfile>();
        for (var i = 0; i < _grid.Rows.Count && i < _profiles.Count; i++)
        {
            if (_grid.Rows[i].Cells["Backup"].Value is true)
                selected.Add(_profiles[i]);
        }
        return selected;
    }

    private static void StyleButton(Button btn, string text, Color backColor)
    {
        btn.Text = text;
        btn.Size = new Size(108, 36);
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = backColor;
        btn.ForeColor = Color.White;
        btn.Cursor = Cursors.Hand;
        btn.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
    }

    private static void StyleSmallButton(Button btn, string text, Color backColor)
    {
        btn.Text = text;
        btn.Size = new Size(90, 28);
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = backColor;
        btn.ForeColor = Color.White;
        btn.Cursor = Cursors.Hand;
        btn.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
    }

    private async Task LoadProfilesAsync()
    {
        if (_busy || _loading) return;
        SetLoading(true, "Profiller okunuyor...");
        _grid.Rows.Clear();

        try
        {
            _profiles = await Task.Run(() => _service.GetProfiles(includeSizes: false));
            PopulateGrid(_profiles, sizesPending: true);
            _profileCountLabel.Text = $"Toplam {_profiles.Count} profil — boyutlar hesaplanıyor...";
            SetLoading(false);

            await Task.Run(() =>
            {
                foreach (var profile in _profiles)
                    _service.EnrichProfileSizes(profile);
            });

            PopulateGrid(_profiles);
            UpdateProfileSummaryLabel();
            AppendLog($"Profil listesi güncellendi ({_profiles.Count} profil).");
            await LoadBackupsAsync();
        }
        catch (Exception ex)
        {
            _profileCountLabel.Text = "Profil bulunamadı";
            AppendLog($"Hata: {ex.Message}");
            MessageBox.Show(ex.Message, "Profil Okuma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            SetLoading(false);
            UseWaitCursor = false;
            Cursor = Cursors.Default;
        }
    }

    private void UpdateProfileSummaryLabel()
    {
        var selected = GetSelectedProfiles();
        var totalDisk = _profiles.Sum(p => p.DiskSizeBytes);
        var totalBackup = selected.Sum(p => p.BackupSizeBytes);
        var cache = totalDisk - _profiles.Sum(p => p.BackupSizeBytes);
        var totalAccounts = _profiles.Sum(p => p.Accounts.Count);

        _profileCountLabel.Text =
            $"Toplam {_profiles.Count} profil  |  Yedeklenecek: {selected.Count}  |  {totalAccounts} e-posta  |  " +
            $"Seçili yedek: {ChromeService.FormatSize(totalBackup)}  |  " +
            $"Önbellek hariç tutulur (~{ChromeService.FormatSize(Math.Max(0, cache))})";
    }

    private void PopulateGrid(List<ChromeProfile> profiles, bool sizesPending = false)
    {
        _grid.Rows.Clear();
        for (var i = 0; i < profiles.Count; i++)
        {
            var p = profiles[i];
            _grid.Rows.Add(
                true,
                i + 1,
                p.Name,
                p.EmailDisplay,
                p.BookmarkCount,
                p.PasswordCount,
                p.ExtensionCount,
                sizesPending ? "..." : ChromeService.FormatSize(p.BackupSizeBytes),
                sizesPending ? "..." : ChromeService.FormatSize(p.DiskSizeBytes),
                p.LastActive?.ToString("dd.MM.yy HH:mm") ?? "-");
        }
    }

    private void GridCellToolTipTextNeeded(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _profiles.Count) return;
        if (_grid.Columns[e.ColumnIndex].Name != "Email") return;

        var accounts = _profiles[e.RowIndex].Accounts;
        if (accounts.Count <= 1) return;

        e.ToolTipText = string.Join(Environment.NewLine,
            accounts.Select(a => $"[{a.AccountKind}{(a.IsPrimary ? " • Ana" : "")}] {a.Email}"));
    }

    private void OpenProfileDetail()
    {
        if (_grid.CurrentRow == null || _grid.CurrentRow.Index < 0 || _grid.CurrentRow.Index >= _profiles.Count)
        {
            MessageBox.Show("Önce bir profil seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var profile = _profiles[_grid.CurrentRow.Index];
        using var form = new ProfileDetailForm(_service, profile);
        form.ShowDialog(this);
    }

    private void BrowseBackupFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Yedeklerin kaydedileceği klasörü seçin",
            SelectedPath = Directory.Exists(_service.BackupRoot) ? _service.BackupRoot : "",
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        _service.SetBackupRoot(dialog.SelectedPath);
        _backupPathBox.Text = _service.BackupRoot;
        AppendLog($"Yedek klasörü: {_service.BackupRoot}");
        _ = LoadBackupsAsync();
    }

    private async Task LoadBackupsAsync()
    {
        _backupCombo.Items.Clear();
        var backups = await Task.Run(() => _service.GetBackups());

        if (backups.Count == 0)
        {
            _backupCombo.Items.Add("(Henüz yedek yok — önce Yedekle'ye tıklayın)");
            _backupCombo.SelectedIndex = 0;
            _backupCombo.Enabled = false;
            return;
        }

        _backupCombo.Enabled = true;
        foreach (var b in backups)
        {
            var label = $"{b.FolderName}  —  {b.ProfileCount} profil  —  {b.Created:dd.MM.yyyy HH:mm}";
            _backupCombo.Items.Add(new BackupComboItem(b, label));
        }
        _backupCombo.DisplayMember = "Display";
        _backupCombo.SelectedIndex = 0;
    }

    private void ShowBackupDetail(bool confirmMode)
    {
        if (_profiles.Count == 0)
        {
            MessageBox.Show("Önce profilleri yükleyin (Yenile).", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selected = GetSelectedProfiles();
        if (selected.Count == 0)
        {
            MessageBox.Show("Yedeklemek için en az bir profil seçin (Yedekle sütunu).", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var options = _service.GetBackupOptions();
        var preview = _service.BuildBackupPreview(selected, options, _service.BackupRoot);
        using var form = new BackupDetailForm(preview, confirmMode);
        form.ShowDialog(this);
    }

    private async Task RunBackupAsync()
    {
        if (_busy) return;

        if (!Directory.Exists(_service.ChromeUserData))
        {
            MessageBox.Show("Chrome verisi bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_profiles.Count == 0)
        {
            MessageBox.Show("Önce profilleri yükleyin (Yenile).", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selected = GetSelectedProfiles();
        if (selected.Count == 0)
        {
            MessageBox.Show("Yedeklemek için en az bir profil seçin (Yedekle sütunu).", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var options = _service.GetBackupOptions();
        var preview = _service.BuildBackupPreview(selected, options, _service.BackupRoot);
        using (var detailForm = new BackupDetailForm(preview, confirmMode: true))
        {
            if (detailForm.ShowDialog(this) != DialogResult.OK || !detailForm.StartBackupConfirmed)
                return;
            options = detailForm.Options;
        }

        if (_service.IsChromeRunning())
        {
            var answer = MessageBox.Show(
                "Chrome açık. Yedekleme için kapatılması gerekiyor.\nDevam edilsin mi?",
                "Chrome Açık", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;
        }

        SetBusy(true, "Yedekleme başlıyor...", waitCursor: true);
        _progress.Visible = true;
        _progress.Value = 0;

        var progress = new Progress<string>(msg => { _statusLabel.Text = msg; AppendLog(msg); });
        var percent = new Progress<int>(v => _progress.Value = Math.Clamp(v, 0, 100));

        try
        {
            var backupDir = await Task.Run(() =>
                _service.BackupAll(selected, options, progress, percent, _service.BackupRoot));
            AppendLog($"Yedek klasörü: {backupDir}");
            MessageBox.Show(
                $"Yedekleme tamamlandı!\n\n{backupDir}\n\nBu klasörü USB veya buluta kopyalayın.",
                "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadBackupsAsync();
        }
        catch (Exception ex)
        {
            AppendLog($"Yedekleme hatası: {ex.Message}");
            MessageBox.Show(ex.Message, "Yedekleme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _progress.Visible = false;
            SetBusy(false);
        }
    }

    private async Task RunRestoreAsync()
    {
        if (_busy) return;

        if (_backupCombo.SelectedItem is not BackupComboItem item)
        {
            MessageBox.Show("Geri yüklenecek bir yedek seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Seçilen yedekten profil seçerek geri yükleyebilirsiniz.\n\n{item.Info.FolderName}\n" +
            $"{item.Info.ProfileCount} profil — {item.Info.Created:dd.MM.yyyy HH:mm}\n\nDevam edilsin mi?",
            "Geri Yükleme", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        List<string> selectedFolders;
        using (var selectForm = new RestoreSelectForm(item.Info.FolderPath))
        {
            if (selectForm.ShowDialog(this) != DialogResult.OK)
                return;
            selectedFolders = selectForm.SelectedFolders;
        }

        if (_service.IsChromeRunning())
        {
            var answer = MessageBox.Show(
                "Chrome açık. Geri yükleme için kapatılması gerekiyor.\nDevam edilsin mi?",
                "Chrome Açık", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;
        }

        SetBusy(true, "Geri yükleme başlıyor...", waitCursor: true);
        _progress.Visible = true;
        _progress.Value = 0;

        var progress = new Progress<string>(msg => { _statusLabel.Text = msg; AppendLog(msg); });
        var percent = new Progress<int>(v => _progress.Value = Math.Clamp(v, 0, 100));

        try
        {
            await Task.Run(() => _service.Restore(item.Info.FolderPath, selectedFolders, progress, percent));
            MessageBox.Show(
                $"{selectedFolders.Count} profil geri yüklendi.\n\nChrome'u açın — profilleriniz görünmeli.",
                "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadProfilesAsync();
        }
        catch (Exception ex)
        {
            AppendLog($"Geri yükleme hatası: {ex.Message}");
            MessageBox.Show(ex.Message, "Geri Yükleme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _progress.Visible = false;
            SetBusy(false);
        }
    }

    private void OpenBackupCompare()
    {
        var backups = _service.GetBackups();
        if (backups.Count < 2)
        {
            MessageBox.Show("Karşılaştırmak için en az 2 yedek gerekli.", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var form = new BackupCompareForm(backups);
        form.ShowDialog(this);
    }

    private void OpenScheduleBackup()
    {
        var exePath = Application.ExecutablePath;
        using var form = new ScheduleBackupForm(exePath);
        form.ShowDialog(this);
    }

    private void OpenBackupFolder()
    {
        Directory.CreateDirectory(_service.BackupRoot);
        Process.Start(new ProcessStartInfo { FileName = _service.BackupRoot, UseShellExecute = true });
    }

    private void SetLoading(bool loading, string? status = null)
    {
        _loading = loading;
        if (!_busy)
        {
            _btnRefresh.Enabled = !loading;
            _btnBackup.Enabled = !loading;
            _btnRestore.Enabled = !loading;
            _btnDetail.Enabled = !loading;
            _btnBackupDetail.Enabled = !loading;
            _btnCompare.Enabled = !loading;
            _btnSchedule.Enabled = !loading;
            _btnOpenBackupFolder.Enabled = !loading;
            _btnBrowseBackup.Enabled = !loading;
        }
        if (status != null) _statusLabel.Text = status;
    }

    private void SetBusy(bool busy, string? status = null, bool waitCursor = false)
    {
        _busy = busy;
        _btnRefresh.Enabled = !busy && !_loading;
        _btnBackup.Enabled = !busy && !_loading;
        _btnRestore.Enabled = !busy && !_loading;
        _btnDetail.Enabled = !busy && !_loading;
        _btnBackupDetail.Enabled = !busy && !_loading;
        _btnCompare.Enabled = !busy && !_loading;
        _btnSchedule.Enabled = !busy && !_loading;
        _btnOpenBackupFolder.Enabled = !busy && !_loading;
        _btnBrowseBackup.Enabled = !busy && !_loading;
        if (status != null) _statusLabel.Text = status;

        if (waitCursor)
        {
            UseWaitCursor = busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }
        else if (!busy)
        {
            UseWaitCursor = false;
            Cursor = Cursors.Default;
        }
    }

    private void AppendLog(string message)
    {
        if (_logBox.TextLength > 8000) _logBox.Clear();
        _logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private sealed class BackupComboItem(BackupInfo info, string display)
    {
        public BackupInfo Info { get; } = info;
        public string Display { get; } = display;
        public override string ToString() => Display;
    }
}
