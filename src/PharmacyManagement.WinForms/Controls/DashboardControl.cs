using PharmacyManagement.Application.DTOs.Dashboard;
using PharmacyManagement.WinForms.Services;

namespace PharmacyManagement.WinForms.Controls;

public sealed class DashboardControl : UserControl
{
    private readonly ApiClient _api;
    private readonly Dictionary<string, Label> _metrics = new();
    private readonly DataGridView _recentSales = CreateGrid();
    private readonly DataGridView _lowStock = CreateGrid();
    private readonly DataGridView _expiring = CreateGrid();

    public DashboardControl(ApiClient api)
    {
        _api = api;
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(244, 247, 249);

        var headerPanel = new Panel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(24, 12, 24, 0) };
        var header = new Label
        {
            Text = "Dashboard",
            Dock = DockStyle.Left,
            Width = 200,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(11, 107, 91)
        };
        var refresh = new Button
        {
            Text = "Refresh",
            Dock = DockStyle.Right,
            Width = 90,
            Height = 32
        };
        refresh.Click += async (_, _) => await LoadAsync();
        headerPanel.Controls.Add(refresh);
        headerPanel.Controls.Add(header);

        var metrics = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 112,
            ColumnCount = 4,
            RowCount = 1,
            Padding = new Padding(18, 10, 18, 8)
        };
        for (var i = 0; i < 4; i++)
        {
            metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }

        metrics.Controls.Add(CreateMetric("Sales today", "SalesToday", _metrics), 0, 0);
        metrics.Controls.Add(CreateMetric("Sales this month", "SalesMonth", _metrics), 1, 0);
        metrics.Controls.Add(CreateMetric("Profit this month", "Profit", _metrics), 2, 0);
        metrics.Controls.Add(CreateMetric("Low stock", "LowStock", _metrics), 3, 0);

        var sections = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            Padding = new Padding(18, 4, 18, 18)
        };
        for (var i = 0; i < 3; i++)
        {
            sections.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        }
        sections.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        sections.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        AddSection(sections, "Recent sales", _recentSales, 0);
        AddSection(sections, "Low stock", _lowStock, 1);
        AddSection(sections, "Expiring soon", _expiring, 2);

        Controls.Add(sections);
        Controls.Add(metrics);
        Controls.Add(headerPanel);

        Load += async (_, _) => await LoadAsync();
    }

    private static Control CreateMetric(string label, string key, Dictionary<string, Label> metrics)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(8),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        panel.Controls.Add(new Label
        {
            Text = label.ToUpperInvariant(),
            Dock = DockStyle.Top,
            Height = 28,
            Padding = new Padding(12, 8, 0, 0),
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            ForeColor = Color.Gray
        });
        var value = new Label
        {
            Name = key,
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 28, 8, 4),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = Color.FromArgb(8, 76, 64)
        };
        panel.Controls.Add(value);
        metrics[key] = value;
        return panel;
    }

    private static void AddSection(TableLayoutPanel parent, string title, DataGridView grid, int column)
    {
        parent.Controls.Add(new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, column, 0);
        grid.Dock = DockStyle.Fill;
        grid.Margin = new Padding(0, 0, 10, 10);
        parent.Controls.Add(grid, column, 1);
    }

    private static DataGridView CreateGrid()
    {
        var grid = new DataGridView
        {
            AutoGenerateColumns = true,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        grid.DataError += (_, e) => { e.ThrowException = false; };
        return grid;
    }

    private async Task LoadAsync()
    {
        try
        {
            var data = await _api.GetAsync<DashboardDto>("dashboard");
            _metrics["SalesToday"].Text = data.TotalSalesToday.ToString("C");
            _metrics["SalesMonth"].Text = data.TotalSalesThisMonth.ToString("C");
            _metrics["Profit"].Text = data.TotalProfitThisMonth.ToString("C");
            _metrics["LowStock"].Text = data.LowStockProducts.ToString();
            _recentSales.DataSource = data.RecentTransactions.ToList();
            _lowStock.DataSource = data.LowStockItems.ToList();
            _expiring.DataSource = data.ExpiringItems.ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
