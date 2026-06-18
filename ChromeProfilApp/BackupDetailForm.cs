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
        var header = BackupDetailLayoutBuilder.CreateHeader(preview);
        var tabs = BackupDetailLayoutBuilder.CreateTabs(preview, _includedLabel, _excludedLabel,
            _chkPasswords, _chkHistory, _chkCookies, _chkExtensions, _chkCache);
        var footer = BackupDetailLayoutBuilder.CreateFooter(confirmMode,
            () => StartConfirmed(),
            () => Close());

        Controls.Add(tabs);
        Controls.Add(footer);
        Controls.Add(header);
    }

    private void StartConfirmed()
    {
        Options = ReadOptions();
        StartBackupConfirmed = true;
        DialogResult = DialogResult.OK;
        Close();
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
}
