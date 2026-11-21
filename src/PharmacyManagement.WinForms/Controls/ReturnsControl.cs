using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Returns;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.WinForms.Forms;
using PharmacyManagement.WinForms.Services;

namespace PharmacyManagement.WinForms.Controls;

public sealed class ReturnsControl : UserControl
{
    private readonly ApiClient _api;
    private readonly EntityListControl<ReturnDto, CreateReturnRequest> _list;

    public ReturnsControl(ApiClient api)
    {
        _api = api;
        Dock = DockStyle.Fill;

        _list = new EntityListControl<ReturnDto, CreateReturnRequest>(
            api, "returns", x => x.Id,
            api.CurrentUser?.Permissions.Contains(Permissions.ReturnsCreate) == true,
            false, false, "Returns")
        {
            CreateRequestFactory = () => new CreateReturnRequest
            {
                BranchId = api.CurrentUser?.BranchId ?? 1,
                Type = ReturnType.SaleReturn,
                OriginalSaleId = 1,
                Reason = "Customer exchange or return",
                RefundMethod = PaymentMethod.Cash,
                Items = new List<ReturnItemRequest>
                {
                    new()
                    {
                        MedicineId = 1,
                        BatchId = 1,
                        Quantity = 1,
                        Condition = ReturnCondition.Resalable,
                        RestockDecision = RestockDecision.Restock
                    }
                }
            }
        };

        _list.AddToolbarItems(
            new ToolStripButton("Details", null, (_, _) => _ = ShowDetailsAsync()),
            new ToolStripButton("Approve", null, (_, _) => _ = ApproveAsync()),
            new ToolStripButton("Reject", null, (_, _) => _ = RejectAsync()));

        Controls.Add(_list);
    }

    private async Task ShowDetailsAsync()
    {
        if (_list.SelectedItem is null) return;
        try
        {
            var item = await _api.GetAsync<ReturnDto>($"returns/{_list.SelectedItem.Id}");
            using var form = new ObjectEditorForm(item, "Return " + item.ReturnNumber, readOnly: true);
            form.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Return", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ApproveAsync()
    {
        if (_list.SelectedItem is null) return;
        var request = new ApproveReturnRequest();
        using var form = new ObjectEditorForm(request, "Approve return");
        if (form.ShowDialog() != DialogResult.OK) return;
        await ActionAsync("approve", request);
    }

    private async Task RejectAsync()
    {
        if (_list.SelectedItem is null) return;
        var request = new RejectReturnRequest { Reason = "Rejected by manager" };
        using var form = new ObjectEditorForm(request, "Reject return");
        if (form.ShowDialog() != DialogResult.OK) return;
        await ActionAsync("reject", request);
    }

    private async Task ActionAsync(string action, object body)
    {
        try
        {
            await _api.PostAsync<object>($"returns/{_list.SelectedItem!.Id}/{action}", body);
            await _list.LoadAsync();
            MessageBox.Show($"Return {action} completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Returns", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
