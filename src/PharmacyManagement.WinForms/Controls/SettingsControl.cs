using PharmacyManagement.Application.DTOs.Settings;
using PharmacyManagement.WinForms.Forms;
using PharmacyManagement.WinForms.Services;

namespace PharmacyManagement.WinForms.Controls;

public sealed class SettingsControl : UserControl
{
    private readonly ApiClient _api;
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        RowHeadersVisible = false,
        AutoGenerateColumns = true,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    public SettingsControl(ApiClient api)
    {
        _api = api;
        Dock = DockStyle.Fill;

        _grid.DataError += (_, e) => { e.ThrowException = false; };
        _grid.DoubleClick += (_, _) => _ = EditAsync();

        var toolbar = new ToolStrip { Dock = DockStyle.Top, GripStyle = ToolStripGripStyle.Hidden };
        toolbar.Items.Add(new ToolStripButton("Refresh", null, (_, _) => _ = LoadAsync()));
        toolbar.Items.Add(new ToolStripButton("Edit", null, (_, _) => _ = EditAsync()));

        Controls.Add(_grid);
        Controls.Add(toolbar);
        Controls.Add(new Label
        {
            Text = "System Settings",
            Dock = DockStyle.Top,
            Height = 46,
            Padding = new Padding(18, 12, 0, 0),
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = Color.FromArgb(11, 107, 91)
        });
        Load += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var settings = await _api.GetAsync<IReadOnlyList<SystemSettingDto>>("settings");
            _grid.DataSource = settings.ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task EditAsync()
    {
        if (_grid.CurrentRow?.DataBoundItem is not SystemSettingDto setting)
        {
            MessageBox.Show("Select a setting first.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var request = new UpdateSystemSettingRequest { Key = setting.Key, Value = setting.Value };
        using var form = new ObjectEditorForm(request, "Edit setting " + setting.Key);
        if (form.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        try
        {
            await _api.PutAsync<object>("settings", request);
            await LoadAsync();
            MessageBox.Show("Setting updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
