namespace ChromeProfilApp;

using ChromeProfilApp.Helpers;

public sealed class ScheduleBackupForm : Form
{
    public ScheduleBackupForm(string exePath)
    {
        Text = "Zamanlanmış Yedek";
        AppIconHelper.Apply(this);
        Size = new Size(480, 280);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10F);

        var info = ScheduleBackupFormLayoutBuilder.CreateInfoLabel();
        var (lblFreq, combo) = ScheduleBackupFormLayoutBuilder.CreateFrequencyPanel();
        var (lblTime, time) = ScheduleBackupFormLayoutBuilder.CreateTimePanel();
        var (btnCreate, btnDelete) = ScheduleBackupFormLayoutBuilder.CreateButtonPanel();

        btnCreate.Click += (_, _) =>
        {
            var schedule = combo.SelectedIndex switch
            {
                0 => "/SC DAILY",
                2 => "/SC MONTHLY /D 1",
                _ => "/SC WEEKLY /D SUN"
            };
            var st = time.Value.ToString("HH:mm");
            var args = $"/Create /F /TN \"ChromeProfilYedek\" /TR \"\\\"{exePath}\\\" --otomatik-yedek\" {schedule} /ST {st} /RL LIMITED";
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            proc?.WaitForExit();
            if (proc?.ExitCode == 0)
                MessageBox.Show($"Zamanlanmış görev oluşturuldu.\nHer çalışmada: {combo.Text}, saat {st}", "Başarılı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Görev oluşturulamadı. Yönetici olarak çalıştırmayı deneyin.", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };

        btnDelete.Click += (_, _) =>
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = "/Delete /F /TN \"ChromeProfilYedek\"",
                UseShellExecute = false,
                CreateNoWindow = true
            })?.WaitForExit();
            MessageBox.Show("Görev silindi (varsa).", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        Controls.AddRange(new Control[] { info, lblFreq, combo, lblTime, time, btnCreate, btnDelete });
    }
}

