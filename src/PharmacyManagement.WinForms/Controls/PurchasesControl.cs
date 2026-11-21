using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.WinForms.Forms;
using PharmacyManagement.WinForms.Services;

namespace PharmacyManagement.WinForms.Controls;

public sealed class PurchasesControl : UserControl
{
    private readonly ApiClient _api;
    private readonly EntityListControl<PurchaseOrderListItemDto, CreatePurchaseOrderRequest> _orders;

    public PurchasesControl(ApiClient api)
    {
        _api = api;
        Dock = DockStyle.Fill;

        _orders = new EntityListControl<PurchaseOrderListItemDto, CreatePurchaseOrderRequest>(
            api, "purchase-orders", x => x.Id,
            api.CurrentUser?.Permissions.Contains(Permissions.PurchasesCreate) == true,
            false, false, "Purchase orders")
        {
            CreateRequestFactory = () => new CreatePurchaseOrderRequest
            {
                SupplierId = 1,
                BranchId = api.CurrentUser?.BranchId ?? 1,
                ExpectedDeliveryDate = DateTime.Today.AddDays(7),
                Items = new List<PurchaseOrderItemRequest>
                {
                    new() { MedicineId = 1, OrderedQuantity = 100, UnitPrice = 5m, TaxPercentage = 0m }
                }
            }
        };

        var receipts = new EntityListControl<GoodsReceiptDto, GoodsReceiptDto>(api, "goods-receipts", x => x.Id, false, false, false, "Goods receipts");

        _orders.AddToolbarItems(
            new ToolStripButton("Details", null, (_, _) => _ = ShowDetailsAsync()),
            new ToolStripButton("Submit", null, (_, _) => _ = ActionAsync("submit", null)),
            new ToolStripButton("Approve", null, (_, _) => _ = ActionAsync("approve", null)),
            new ToolStripButton("Reject", null, (_, _) => _ = ActionAsync("reject", new { reason = "Rejected by user" })),
            new ToolStripButton("Receive", null, (_, _) => _ = ReceiveAsync()),
            new ToolStripButton("Cancel", null, (_, _) => _ = ActionAsync("cancel", null)));

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateTab("Orders", _orders));
        tabs.TabPages.Add(CreateTab("Receipts", receipts));
        tabs.SelectedIndexChanged += async (_, _) =>
        {
            if (tabs.SelectedIndex == 0) await _orders.LoadAsync();
            else if (tabs.SelectedIndex == 1) await receipts.LoadAsync();
        };
        Controls.Add(tabs);
    }

    private static TabPage CreateTab(string title, Control control)
    {
        var page = new TabPage(title);
        control.Dock = DockStyle.Fill;
        page.Controls.Add(control);
        return page;
    }

    private async Task ShowDetailsAsync()
    {
        if (_orders.SelectedItem is null) return;
        try
        {
            var order = await _api.GetAsync<PurchaseOrderDto>($"purchase-orders/{_orders.SelectedItem.Id}");
            using var form = new ObjectEditorForm(order, "Purchase order " + order.OrderNumber, readOnly: true);
            form.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Purchase order", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ReceiveAsync()
    {
        if (_orders.SelectedItem is null) return;
        var order = await _api.GetAsync<PurchaseOrderDto>($"purchase-orders/{_orders.SelectedItem.Id}");
        var request = new ReceivePurchaseOrderRequest
        {
            Items = order.Details
                .Where(d => d.PendingQuantity > 0)
                .Select(d => new ReceivePurchaseOrderItemRequest
                {
                    PurchaseOrderDetailId = d.Id,
                    Quantity = d.PendingQuantity,
                    BatchNumber = string.IsNullOrWhiteSpace(d.BatchNumber) ? $"BAT-{order.OrderNumber}-{d.Id}" : d.BatchNumber,
                    ManufacturingDate = d.ManufacturingDate ?? DateTime.Today.AddMonths(-1),
                    ExpiryDate = d.ExpiryDate ?? DateTime.Today.AddYears(2),
                    UnitCost = d.UnitPrice
                }).ToList()
        };

        if (request.Items.Count == 0)
        {
            MessageBox.Show("All items in this order have already been received.", "Receive Goods", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new ObjectEditorForm(request, "Receive goods - " + order.OrderNumber);
        if (form.ShowDialog() != DialogResult.OK) return;
        await ActionAsync("receive", request);
    }

    private async Task ActionAsync(string action, object? body)
    {
        if (_orders.SelectedItem is null) return;
        try
        {
            await _api.PostAsync<object>($"purchase-orders/{_orders.SelectedItem.Id}/{action}", body);
            await _orders.LoadAsync();
            MessageBox.Show($"Order {action} completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Purchasing", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
