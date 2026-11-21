namespace PharmacyManagement.WinForms.Forms;

public sealed class ObjectEditorForm : Form
{
    private readonly PropertyGrid _grid = new();

    public ObjectEditorForm(object value, string title, bool readOnly = false)
    {
        Text = title;
        Width = 620;
        Height = 680;
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(520, 520);

        _grid.Dock = DockStyle.Fill;
        _grid.SelectedObject = value;
        _grid.PropertySort = PropertySort.NoSort;
        _grid.ToolbarVisible = false;
        _grid.HelpVisible = false;
        _grid.Font = new Font("Segoe UI", 10F);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 52,
            Padding = new Padding(12, 8, 12, 8)
        };

        var ok = new Button { Text = "Save", Width = 100, Height = 34, DialogResult = DialogResult.OK, Visible = !readOnly };
        ok.Click += (_, _) => buttons.Focus();
        var cancel = new Button { Text = readOnly ? "Close" : "Cancel", Width = 100, Height = 34, DialogResult = DialogResult.Cancel };
        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);

        Controls.Add(_grid);
        Controls.Add(buttons);
        AcceptButton = readOnly ? cancel : ok;
        CancelButton = cancel;
    }
}
