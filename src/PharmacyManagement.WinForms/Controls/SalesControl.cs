using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.WinForms.Forms;
using PharmacyManagement.WinForms.Services;

namespace PharmacyManagement.WinForms.Controls;

public sealed class SalesControl : UserControl
{
    private readonly ApiClient _api;
    private readonly EntityListControl<SaleListItemDto, CreateSaleRequest> _list;

    public SalesControl(ApiClient api)
    {
        _api = api;
        Dock = DockStyle.Fill;

        _list = new EntityListControl<SaleListItemDto, CreateSaleRequest>(
            api, "sales", x => x.Id, false, false, false, "Sales");

        var details = new ToolStripButton("Details", null, (_, _) => _ = ShowDetailsAsync());
        var invoice = new ToolStripButton("Invoice", null, (_, _) => _ = ShowInvoiceAsync());
        var payment = new ToolStripButton("Record payment", null, (_, _) => _ = RecordPaymentAsync());
        var cancel = new ToolStripButton("Cancel sale", null, (_, _) => _ = CancelAsync())
        {
            Visible = api.CurrentUser?.Permissions.Contains(Permissions.SalesCancel) == true
        };

        _list.AddToolbarItems(details, invoice, payment, cancel);
        Controls.Add(_list);
    }

    private async Task ShowInvoiceAsync()
    {
        if (_list.SelectedItem is null) return;
        try
        {
            var data = await _api.GetAsync<SaleInvoiceDto>($"sales/{_list.SelectedItem.Id}/invoice");
            using var form = new ObjectEditorForm(data, "Invoice " + data.Sale.InvoiceNumber, readOnly: true);
            form.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Invoice", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ShowDetailsAsync()
    {
        if (_list.SelectedItem is null)
        {
            return;
        }

        try
        {
            var sale = await _api.GetAsync<SaleDto>($"sales/{_list.SelectedItem.Id}");
            using var form = new ObjectEditorForm(sale, "Sale " + sale.InvoiceNumber, readOnly: true);
            form.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Sale details", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RecordPaymentAsync()
    {
        if (_list.SelectedItem is null)
        {
            return;
        }

        var request = new RecordSalePaymentRequest();
        using var form = new ObjectEditorForm(request, "Record payment");
        if (form.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        try
        {
            await _api.PostAsync<object>($"sales/{_list.SelectedItem.Id}/payments", request);
            await _list.LoadAsync();
            MessageBox.Show("Payment recorded.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Payment", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CancelAsync()
    {
        if (_list.SelectedItem is null)
        {
            return;
        }

        var reason = Prompt("Cancellation reason");
        if (string.IsNullOrWhiteSpace(reason))
        {
            return;
        }

        try
        {
            await _api.PostAsync<object>($"sales/{_list.SelectedItem.Id}/cancel", new { reason });
            await _list.LoadAsync();
            MessageBox.Show("Sale cancelled.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Cancel sale", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string? Prompt(string title)
    {
        using var form = new Form
        {
            Text = title,
            Width = 460,
            Height = 180,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false,
            MaximizeBox = false
        };
        var text = new TextBox { Dock = DockStyle.Top, Margin = new Padding(12), Multiline = true, Height = 70 };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, Height = 36 };
        form.Controls.Add(text);
        form.Controls.Add(ok);
        form.AcceptButton = ok;
        return form.ShowDialog() == DialogResult.OK ? text.Text : null;
    }
}
