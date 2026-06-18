using ChromeProfilApp.Models;

namespace ChromeProfilApp.Services;

public sealed class BackupCoordinator
{
    private readonly ChromeService _service;

    public BackupCoordinator(ChromeService service)
    {
        _service = service;
    }

    public async Task<string?> RunBackupAsync(Form owner, List<ChromeProfile> selected, IProgress<string> progress, IProgress<int> percent)
    {
        var options = _service.GetBackupOptions();
        var preview = _service.BuildBackupPreview(selected, options, _service.BackupRoot);
        using var detailForm = new BackupDetailForm(preview, confirmMode: true);
        if (detailForm.ShowDialog(owner) != DialogResult.OK || !detailForm.StartBackupConfirmed)
            return null;
        options = detailForm.Options;

        if (_service.IsChromeRunning())
        {
            var answer = MessageBox.Show(owner,
                "Chrome açık. Yedekleme için kapatılması gerekiyor.\nDevam edilsin mi?",
                "Chrome Açık", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return null;
        }

        // Run actual backup on background thread
        var backupDir = await Task.Run(() =>
            _service.BackupAll(selected, options, progress, percent, _service.BackupRoot));

        return backupDir;
    }
}
