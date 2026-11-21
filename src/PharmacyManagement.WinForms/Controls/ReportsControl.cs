using System.Data;
using System.Text.Json;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Reports;
using PharmacyManagement.WinForms.Services;

namespace PharmacyManagement.WinForms.Controls;

public sealed class ReportsControl : UserControl
{
    private readonly ApiClient _api;
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 210 };
    private readonly DateTimePicker _from = new() { Format = DateTimePickerFormat.Short, Width = 130 };
    private readonly DateTimePicker _to = new() { Format = DateTimePickerFormat.Short, Width = 130, Value = DateTime.Today };
    private readonly NumericUpDown _branch = new() { Width = 80, Minimum = 0, Maximum = 100000 };
    private readonly DataGridView _grid = CreateGrid();
    private readonly Label _summary = new() { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(12, 10, 0, 0), ForeColor = Color.Gray };
    private string _typeValue = ReportTypes.Sales;

    private sealed record ReportOption(string Value, string Text);

    public ReportsControl(ApiClient api)
    {
        _api = api;
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(244, 247, 249);

        _from.Value = DateTime.Today.AddDays(-30);
        _grid.DataError += (_, e) => { e.ThrowException = false; };
        _branch.Value = api.CurrentUser?.BranchId ?? 0;

        var options = ReportTypes.All.Select(x => new ReportOption(x, Split(x))).ToList();
        _type.DisplayMember = nameof(ReportOption.Text);
        _type.ValueMember = nameof(ReportOption.Value);
        _type.DataSource = options;
        _type.SelectedIndexChanged += (_, _) =>
        {
            if (_type.SelectedItem is ReportOption opt)
            {
                _typeValue = opt.Value;
            }
        };

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(18, 12, 18, 6) };
        toolbar.Controls.Add(new Label { Text = "Report", Width = 54, TextAlign = ContentAlignment.MiddleLeft });
        toolbar.Controls.Add(_type);
        toolbar.Controls.Add(new Label { Text = "From", Width = 45, TextAlign = ContentAlignment.MiddleRight });
        toolbar.Controls.Add(_from);
        toolbar.Controls.Add(new Label { Text = "To", Width = 30, TextAlign = ContentAlignment.MiddleRight });
        toolbar.Controls.Add(_to);
        toolbar.Controls.Add(new Label { Text = "Branch", Width = 55, TextAlign = ContentAlignment.MiddleRight });
        toolbar.Controls.Add(_branch);

        var generate = new Button { Text = "Generate", Width = 100 };
        generate.Click += async (_, _) => await GenerateAsync();
        toolbar.Controls.Add(generate);

        var excel = new Button { Text = "Excel", Width = 80 };
        excel.Click += async (_, _) => await ExportAsync("excel", "xlsx");
        toolbar.Controls.Add(excel);

        var pdf = new Button { Text = "PDF", Width = 70 };
        pdf.Click += async (_, _) => await ExportAsync("pdf", "pdf");
        toolbar.Controls.Add(pdf);

        Controls.Add(_grid);
        Controls.Add(_summary);
        Controls.Add(toolbar);
        Load += async (_, _) => await GenerateAsync();
    }

    private static DataGridView CreateGrid()
        => new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White
        };

    private string FilterQuery()
    {
        var query = $"?from={Uri.EscapeDataString(_from.Value.Date.ToString("yyyy-MM-dd"))}" +
                    $"&to={Uri.EscapeDataString(_to.Value.Date.AddDays(1).AddTicks(-1).ToString("yyyy-MM-ddTHH:mm:ss"))}";
        if (_branch.Value > 0)
        {
            query += $"&branchId={_branch.Value}";
        }
        return query;
    }

    private static object ConvertCell(object? value)
    {
        if (value is null) return DBNull.Value;
        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? (object)DBNull.Value,
                JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.TryGetDecimal(out var d) ? d : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null or JsonValueKind.Undefined => DBNull.Value,
                _ => element.ToString()
            };
        }
        return value;
    }

    private async Task GenerateAsync()
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            var report = await _api.GetAsync<ReportTableDto>("reports/" + _typeValue + FilterQuery());
            var table = new DataTable();
            foreach (var column in report.Columns)
            {
                table.Columns.Add(column);
            }

            foreach (var row in report.Rows)
            {
                var values = new object[table.Columns.Count];
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    values[i] = i < row.Count ? ConvertCell(row[i]) : DBNull.Value;
                }
                table.Rows.Add(values);
            }

            _grid.DataSource = table;
            _summary.Text = $"{report.Title}  |  {report.Subtitle}  |  Generated {report.GeneratedAtUtc:G}  |  {string.Join("   ", report.Summary.Select(x => x.Label + ": " + x.Value))}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Report", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private async Task ExportAsync(string format, string extension)
    {
        try
        {
            using var dialog = new SaveFileDialog
            {
                FileName = $"{_typeValue}-{DateTime.UtcNow:yyyyMMddHHmmss}.{extension}",
                Filter = extension == "pdf" ? "PDF files|*.pdf" : "Excel files|*.xlsx"
            };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var bytes = await _api.GetBytesAsync("reports/export" + FilterQuery() + $"&type={Uri.EscapeDataString(_typeValue)}&format={format}");
            await File.WriteAllBytesAsync(dialog.FileName, bytes);
            MessageBox.Show("Report exported.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string Split(string value)
        => string.Concat(value.Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));
}
