using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.WinForms.Forms;
using PharmacyManagement.WinForms.Services;

namespace PharmacyManagement.WinForms.Controls;

public sealed class InventoryControl : UserControl
{
    private readonly ApiClient _api;
    private readonly EntityListControl<BatchDto, StockAdjustmentRequest> _batches;
    private readonly EntityListControl<StockTransferDto, CreateStockTransferRequest> _transfers;

    public InventoryControl(ApiClient api)
    {
        _api = api;
        Dock = DockStyle.Fill;

        _batches = new EntityListControl<BatchDto, StockAdjustmentRequest>(api, "inventory/batches", x => x.Id, false, false, false, "Batches");
        var stock = new EntityListControl<StockSummaryDto, StockSummaryDto>(api, "inventory/stock", x => x.MedicineId, false, false, false, "Stock on hand");
        var movements = new EntityListControl<InventoryTransactionDto, InventoryTransactionDto>(api, "inventory/movements", x => x.Id, false, false, false, "Stock movements");
        _transfers = new EntityListControl<StockTransferDto, CreateStockTransferRequest>(
            api, "stock-transfers", x => x.Id,
            api.CurrentUser?.Permissions.Contains(Permissions.InventoryTransfer) == true,
            false, false, "Stock transfers")
        {
            CreateRequestFactory = () => new CreateStockTransferRequest
            {
                FromBranchId = 1,
                ToBranchId = 2,
                Notes = "Stock rebalance",
                Items = new List<StockTransferItemRequest>
                {
                    new() { BatchId = 1, Quantity = 5 }
                }
            }
        };

        if (api.CurrentUser?.Permissions.Contains(Permissions.InventoryAdjust) == true)
        {
            _batches.AddToolbarItems(
                new ToolStripButton("Stock in", null, (_, _) => _ = AdjustAsync("stock-in", null)),
                new ToolStripButton("Stock out", null, (_, _) => _ = AdjustAsync("stock-out", null)),
                new ToolStripButton("Adjust", null, (_, _) => _ = AdjustAsync("adjustments", null)));
        }

        _transfers.AddToolbarItems(
            new ToolStripButton("Approve", null, (_, _) => _ = TransferActionAsync("approve")),
            new ToolStripButton("Complete", null, (_, _) => _ = TransferActionAsync("complete")),
            new ToolStripButton("Cancel", null, (_, _) => _ = TransferActionAsync("cancel")));

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateTab("Batches", _batches));
        tabs.TabPages.Add(CreateTab("Stock", stock));
        tabs.TabPages.Add(CreateTab("Movements", movements));
        tabs.TabPages.Add(CreateTab("Transfers", _transfers));
        tabs.SelectedIndexChanged += async (_, _) =>
        {
            if (tabs.SelectedIndex == 0) await _batches.LoadAsync();
            else if (tabs.SelectedIndex == 1) await stock.LoadAsync();
            else if (tabs.SelectedIndex == 2) await movements.LoadAsync();
            else if (tabs.SelectedIndex == 3) await _transfers.LoadAsync();
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

    private async Task AdjustAsync(string action, object? request)
    {
        if (action != "stock-in" && _batches.SelectedItem is null)
        {
            MessageBox.Show("Select a batch first.", "Inventory", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        request ??= action switch
        {
            "stock-in" => new StockInRequest
            {
                MedicineId = _batches.SelectedItem?.MedicineId ?? 1,
                BranchId = _api.CurrentUser?.BranchId ?? 1,
                BatchNumber = $"BAT-{DateTime.UtcNow:yyMMddHHmm}",
                Quantity = 50,
                PurchasePrice = 10m,
                SellingPrice = 15m,
                ManufacturingDate = DateTime.Today.AddMonths(-1),
                ExpiryDate = DateTime.Today.AddYears(2)
            },
            "stock-out" => new StockOutRequest
            {
                BatchId = _batches.SelectedItem!.Id,
                Quantity = 1,
                Type = InventoryTransactionType.Damaged,
                Reason = "Damaged packaging"
            },
            _ => new StockAdjustmentRequest
            {
                BatchId = _batches.SelectedItem!.Id,
                NewQuantity = _batches.SelectedItem!.CurrentQuantity,
                Reason = "Inventory recount adjustment"
            }
        };

        if (request is StockOutRequest stockOut) stockOut.BatchId = _batches.SelectedItem!.Id;
        if (request is StockAdjustmentRequest adjustment) adjustment.BatchId = _batches.SelectedItem!.Id;

        using var form = new ObjectEditorForm(request, action.Replace("-", " "));
        if (form.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        try
        {
            await _api.PostAsync<object>($"inventory/{action}", request);
            await _batches.LoadAsync();
            MessageBox.Show("Inventory updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task TransferActionAsync(string action)
    {
        if (_transfers.SelectedItem is null)
        {
            MessageBox.Show("Select a transfer first.", "Transfers", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            await _api.PostAsync<object>($"stock-transfers/{_transfers.SelectedItem.Id}/{action}", null);
            await _transfers.LoadAsync();
            MessageBox.Show($"Transfer {action}d.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Transfer", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
