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

        var info = new Label
        {
            Location = new Point(16, 16),
            Size = new Size(430, 60),
            Text = "Windows Görev Zamanlayıcı ile otomatik yedek oluşturur.\nChrome açıksa o hafta yedek atlanır."
        };

        var lblFreq = new Label { Text = "Sıklık:", Location = new Point(16, 88), AutoSize = true };
        var combo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(100, 84),
            Size = new Size(200, 28)
        };
        combo.Items.AddRange(["Her gün", "Her hafta (Pazar)", "Her ay (1. gün)"]);
        combo.SelectedIndex = 1;

        var lblTime = new Label { Text = "Saat:", Location = new Point(16, 124), AutoSize = true };
        var time = new DateTimePicker
        {
            Location = new Point(100, 120),
            Size = new Size(100, 28),
            Format = DateTimePickerFormat.Time,
            ShowUpDown = true,
            Value = DateTime.Today.AddHours(2)
        };

        var btnCreate = new Button
        {
            Text = "Görev Oluştur",
            Location = new Point(16, 170),
            Size = new Size(130, 34),
            BackColor = Color.FromArgb(26, 115, 232),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnCreate.FlatAppearance.BorderSize = 0;

        var btnDelete = new Button
        {
            Text = "Görevi Sil",
            Location = new Point(160, 170),
            Size = new Size(100, 34),
            FlatStyle = FlatStyle.Flat
        };

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

        Controls.AddRange([info, lblFreq, combo, lblTime, time, btnCreate, btnDelete]);
    }
}
