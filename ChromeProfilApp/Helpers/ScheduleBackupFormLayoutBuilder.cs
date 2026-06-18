namespace ChromeProfilApp.Helpers;

public static class ScheduleBackupFormLayoutBuilder
{
    public static Label CreateInfoLabel()
    {
        return new Label
        {
            Location = new Point(16, 16),
            Size = new Size(430, 60),
            Text = "Windows Görev Zamanlayıcı ile otomatik yedek oluşturur.\nChrome açıksa o hafta yedek atlanır."
        };
    }

    public static (Label FreqLabel, ComboBox FreqCombo) CreateFrequencyPanel()
    {
        var lblFreq = new Label { Text = "Sıklık:", Location = new Point(16, 88), AutoSize = true };
        var combo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(100, 84),
            Size = new Size(200, 28)
        };
        combo.Items.AddRange(new object[] { "Her gün", "Her hafta (Pazar)", "Her ay (1. gün)" });
        combo.SelectedIndex = 1;

        return (lblFreq, combo);
    }

    public static (Label TimeLabel, DateTimePicker TimePicker) CreateTimePanel()
    {
        var lblTime = new Label { Text = "Saat:", Location = new Point(16, 124), AutoSize = true };
        var time = new DateTimePicker
        {
            Location = new Point(100, 120),
            Size = new Size(100, 28),
            Format = DateTimePickerFormat.Time,
            ShowUpDown = true,
            Value = DateTime.Today.AddHours(2)
        };

        return (lblTime, time);
    }

    public static (Button CreateBtn, Button DeleteBtn) CreateButtonPanel()
    {
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

        return (btnCreate, btnDelete);
    }
}
