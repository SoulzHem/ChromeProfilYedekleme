using ChromeProfilApp.Models;

namespace ChromeProfilApp.Helpers;

internal static class ProfileDetailLayoutBuilder
{
    public static TabControl CreateTabs(Form owner, ChromeProfile profile, DataGridView passwordGrid, DataGridView bookmarkGrid,
        DataGridView accountGrid, DataGridView extensionGrid, DataGridView historyGrid, CheckBox showPasswords,
        Label pwdInfoLabel, out TableLayoutPanel summaryTable, Func<Task> refreshAction)
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
        tabs.TabPages.Add(CreateSummaryTab(out summaryTable));
        tabs.TabPages.Add(CreateAccountsTab(accountGrid));
        tabs.TabPages.Add(CreateExtensionsTab(extensionGrid));
        tabs.TabPages.Add(CreateBookmarksTab(bookmarkGrid, profile.BookmarkCount));
        tabs.TabPages.Add(CreateHistoryTab(historyGrid));
        tabs.TabPages.Add(CreatePasswordsTab(passwordGrid, showPasswords, pwdInfoLabel, refreshAction));
        return tabs;
    }

    private static TabPage CreateSummaryTab(out TableLayoutPanel summaryTable)
    {
        var page = new TabPage("Özet") { BackColor = Color.White, Padding = new Padding(12) };
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
        summaryTable = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(8),
            BackColor = Color.White
        };
        summaryTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        summaryTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        page.Controls.Add(scroll);
        scroll.Controls.Add(summaryTable);
        return page;
    }

    private static TabPage CreateAccountsTab(DataGridView accountGrid)
    {
        var page = new TabPage("E-postalar") { Padding = new Padding(8) };
        var info = new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            ForeColor = Color.FromArgb(95, 99, 104),
            Text = "Google Hesabı = Chrome'a giriş yaptığınız hesap. Site Girişi = sitelere kaydettiğiniz e-posta adresleri."
        };
        accountGrid.Dock = DockStyle.Fill;
        page.Controls.Add(info);
        page.Controls.Add(accountGrid);
        return page;
    }

    private static TabPage CreateExtensionsTab(DataGridView extensionGrid)
    {
        var page = new TabPage("Eklentiler") { Padding = new Padding(8) };
        extensionGrid.Dock = DockStyle.Fill;
        page.Controls.Add(extensionGrid);
        return page;
    }

    private static TabPage CreateBookmarksTab(DataGridView bookmarkGrid, int bookmarkCount)
    {
        var page = new TabPage($"Yer İmleri ({bookmarkCount})") { Padding = new Padding(8) };
        bookmarkGrid.Dock = DockStyle.Fill;
        page.Controls.Add(bookmarkGrid);
        return page;
    }

    private static TabPage CreateHistoryTab(DataGridView historyGrid)
    {
        var page = new TabPage("Geçmiş") { Padding = new Padding(8) };
        historyGrid.Dock = DockStyle.Fill;
        page.Controls.Add(historyGrid);
        return page;
    }

    private static TabPage CreatePasswordsTab(DataGridView passwordGrid, CheckBox showPasswords, Label pwdInfoLabel, Func<Task> refreshAction)
    {
        var page = new TabPage("Şifreler") { Padding = new Padding(8) };
        var top = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = Color.FromArgb(248, 249, 252) };
        var btnRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(8, 8, 8, 0)
        };
        btnRow.Controls.Add(showPasswords);
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
        btnReload.Click += async (_, _) => await refreshAction();
        btnRow.Controls.Add(btnReload);

        pwdInfoLabel.Dock = DockStyle.Top;
        pwdInfoLabel.AutoSize = true;
        pwdInfoLabel.Padding = new Padding(8, 4, 8, 8);
        pwdInfoLabel.ForeColor = Color.FromArgb(95, 99, 104);

        top.Controls.Add(pwdInfoLabel);
        top.Controls.Add(btnRow);
        passwordGrid.Dock = DockStyle.Fill;
        passwordGrid.Dock = DockStyle.Fill;
        page.Controls.Add(top);
        page.Controls.Add(passwordGrid);
        return page;
    }

    public static Panel CreateExportBar(Button btnBookmarks, Button btnPasswords, Button btnAccounts, Button btnHistory)
    {
        var exportBar = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.White, Padding = new Padding(12, 8, 12, 8) };
        var lblExport = new Label { Text = "CSV dışa aktar:", AutoSize = true, Location = new Point(12, 12) };
        btnBookmarks.Location = new Point(120, 6);
        btnPasswords.Location = new Point(220, 6);
        btnAccounts.Location = new Point(310, 6);
        btnHistory.Location = new Point(410, 6);

        exportBar.Controls.AddRange(new Control[] { lblExport, btnBookmarks, btnPasswords, btnAccounts, btnHistory });
        return exportBar;
    }
}
