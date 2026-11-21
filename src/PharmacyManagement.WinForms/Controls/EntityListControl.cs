using System.ComponentModel;
using PharmacyManagement.Application.Common;
using PharmacyManagement.WinForms.Forms;
using PharmacyManagement.WinForms.Services;

namespace PharmacyManagement.WinForms.Controls;

public sealed class EntityListControl<TItem, TRequest> : UserControl
    where TItem : class
    where TRequest : class, new()
{
    private readonly ApiClient _api;
    private readonly BindingList<TItem> _items = new();
    private readonly DataGridView _grid = new();
    private readonly ToolStripTextBox _search = new();
    private readonly ToolStripButton _newButton;
    private readonly ToolStripButton _editButton;
    private readonly ToolStripButton _deleteButton;

    public EntityListControl(
        ApiClient api,
        string endpoint,
        Func<TItem, object> idSelector,
        bool allowCreate = true,
        bool allowEdit = true,
        bool allowDelete = true,
        string title = "")
    {
        _api = api;
        Endpoint = endpoint.Trim('/');
        IdSelector = idSelector;
        Title = title;
        AllowCreate = allowCreate;
        AllowEdit = allowEdit;
        AllowDelete = allowDelete;

        _newButton = new ToolStripButton("New", null, async (_, _) => await CreateAsync()) { Visible = allowCreate };
        _editButton = new ToolStripButton("Edit", null, async (_, _) => await EditAsync()) { Visible = allowEdit };
        _deleteButton = new ToolStripButton("Delete", null, async (_, _) => await DeleteAsync()) { Visible = allowDelete };

        var toolbar = new ToolStrip
        {
            Dock = DockStyle.Top,
            GripStyle = ToolStripGripStyle.Hidden,
            RenderMode = ToolStripRenderMode.System
        };
        toolbar.Items.Add(new ToolStripButton("Refresh", null, async (_, _) => await LoadAsync()));
        toolbar.Items.Add(new ToolStripSeparator());
        toolbar.Items.Add(_newButton);

        toolbar.Items.Add(_editButton);
        toolbar.Items.Add(_deleteButton);
        toolbar.Items.Add(new ToolStripSeparator());
        toolbar.Items.Add(new ToolStripLabel("Search:"));
        toolbar.Items.Add(_search);
        toolbar.Items.Add(new ToolStripButton("Find", null, async (_, _) => await LoadAsync()));
        _search.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                _ = LoadAsync();
            }
        };

        _grid.Dock = DockStyle.Fill;
        _grid.DataSource = _items;
        _grid.AutoGenerateColumns = true;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        _grid.RowHeadersVisible = false;
        _grid.DataError += (_, e) => { e.ThrowException = false; };
        _grid.DoubleClick += (_, _) => { if (AllowEdit) _ = EditAsync(); };

        var header = new Label
        {
            Text = Title,
            Dock = DockStyle.Top,
            Height = 46,
            Padding = new Padding(18, 12, 0, 0),
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = Color.FromArgb(11, 107, 91)
        };

        Controls.Add(_grid);
        Controls.Add(toolbar);
        Controls.Add(header);
    }

    public string Endpoint { get; }
    public string Title { get; }
    public Func<TItem, object> IdSelector { get; }
    public bool AllowCreate { get; }
    public bool AllowEdit { get; }
    public bool AllowDelete { get; }
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<TItem, TRequest>? MapToRequest { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<TItem, Task<TRequest>>? ResolveEditRequestAsync { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<TRequest>? CreateRequestFactory { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<TRequest, Task<bool>>? CustomCreateHandlerAsync { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<TItem, TRequest, Task<bool>>? CustomEditHandlerAsync { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<TRequest, bool>? ValidateRequest { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<KeyValuePair<string, string?>>? ExtraQuery { get; set; }
    public TItem? SelectedItem => _grid.CurrentRow?.DataBoundItem as TItem;

    public event EventHandler? DataLoaded;

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (!DesignMode)
        {
            await LoadAsync();
        }
    }

    public async Task LoadAsync()
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            var query = new QueryParameters { Page = 1, PageSize = 500, Search = _search.Text };
            var result = await _api.GetPagedAsync<TItem>(Endpoint, query, ExtraQuery);
            _items.Clear();
            foreach (var item in result.Items)
            {
                _items.Add(item);
            }

            if (_grid.Columns.Count > 0)
            {
                _grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
            }

            DataLoaded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    public void AddToolbarItems(params ToolStripItem[] items)
    {
        if (FindControls<ToolStrip>().FirstOrDefault() is { } toolbar)
        {
            foreach (var item in items.Reverse())
            {
                toolbar.Items.Insert(3, item);
            }
        }
    }

    private async Task CreateAsync()
    {
        var request = CreateRequestFactory?.Invoke() ?? new TRequest();
        if (!ShowEditor(request, "Create " + SingularTitle()))
        {
            return;
        }

        if (ValidateRequest?.Invoke(request) == false)
        {
            return;
        }

        await ExecuteAsync(async () =>
        {
            if (CustomCreateHandlerAsync is not null)
            {
                var handled = await CustomCreateHandlerAsync(request);
                if (handled)
                {
                    await LoadAsync();
                    MessageBox.Show("Record created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            await _api.PostAsync<object>(Endpoint, request);
            await LoadAsync();
            MessageBox.Show("Record created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private async Task EditAsync()
    {
        if (SelectedItem is null)
        {
            MessageBox.Show("Select a record first.", "Edit", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        TRequest request;
        try
        {
            Cursor = Cursors.WaitCursor;
            request = ResolveEditRequestAsync is not null
                ? await ResolveEditRequestAsync(SelectedItem)
                : MapToRequest?.Invoke(SelectedItem) ?? ObjectMapper.Copy<TItem, TRequest>(SelectedItem);
        }
        catch (Exception ex)
        {
            ShowError(ex);
            return;
        }
        finally
        {
            Cursor = Cursors.Default;
        }

        if (!ShowEditor(request, "Edit " + SingularTitle()))
        {
            return;
        }

        if (ValidateRequest?.Invoke(request) == false)
        {
            return;
        }

        await ExecuteAsync(async () =>
        {
            if (CustomEditHandlerAsync is not null)
            {
                var handled = await CustomEditHandlerAsync(SelectedItem, request);
                if (handled)
                {
                    await LoadAsync();
                    MessageBox.Show("Record updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            await _api.PutAsync<object>($"{Endpoint}/{IdSelector(SelectedItem)}", request);
            await LoadAsync();
            MessageBox.Show("Record updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private async Task DeleteAsync()
    {
        if (SelectedItem is null)
        {
            MessageBox.Show("Select a record first.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var answer = MessageBox.Show("Delete this record?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes)
        {
            return;
        }

        await ExecuteAsync(async () =>
        {
            await _api.DeleteAsync($"{Endpoint}/{IdSelector(SelectedItem)}");
            await LoadAsync();
            MessageBox.Show("Record deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            await action();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private static bool ShowEditor(object request, string title)
    {
        using var form = new ObjectEditorForm(request, title);
        return form.ShowDialog() == DialogResult.OK;
    }

    private static void ShowError(Exception exception)
    {
        var message = exception is ApiException ? exception.Message : "Unexpected error: " + exception.Message;
        MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private string SingularTitle()
    {
        var text = string.IsNullOrWhiteSpace(Title) ? typeof(TItem).Name : Title;
        return text.TrimEnd('s');
    }

    private IEnumerable<T> FindControls<T>() where T : Control
    {
        foreach (Control control in Controls)
        {
            if (control is T match)
            {
                yield return match;
            }
        }
    }
}
