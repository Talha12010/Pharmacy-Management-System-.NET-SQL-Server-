using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Prescriptions;
using PharmacyManagement.WinForms.Forms;
using PharmacyManagement.WinForms.Services;

namespace PharmacyManagement.WinForms.Controls;

public sealed class PrescriptionsControl : UserControl
{
    private readonly ApiClient _api;
    private readonly EntityListControl<PrescriptionDto, CreatePrescriptionRequest> _list;

    public PrescriptionsControl(ApiClient api)
    {
        _api = api;
        Dock = DockStyle.Fill;

        _list = new EntityListControl<PrescriptionDto, CreatePrescriptionRequest>(
            api, "prescriptions", x => x.Id,
            api.CurrentUser?.Permissions.Contains(Permissions.PrescriptionsCreate) == true,
            false, false, "Prescriptions")
        {
            CreateRequestFactory = () => new CreatePrescriptionRequest
            {
                BranchId = api.CurrentUser?.BranchId ?? 1,
                PrescriptionDate = DateTime.Today,
                PatientName = "Walk-in Patient",
                DoctorName = "Dr. Clinic",
                Details = new List<PrescriptionItemRequest>
                {
                    new() { MedicineId = 1, Dosage = "1 tablet daily", Quantity = 10, DurationDays = 10 }
                }
            }
        };

        _list.AddToolbarItems(
            new ToolStripButton("Details", null, (_, _) => _ = ShowDetailsAsync()),
            new ToolStripButton("Verify", null, (_, _) => _ = ActionAsync("verify", null)),
            new ToolStripButton("Reject", null, (_, _) => _ = ActionAsync("reject", new { reason = "Rejected by pharmacist" })));

        Controls.Add(_list);
    }

    private async Task ShowDetailsAsync()
    {
        if (_list.SelectedItem is null) return;
        try
        {
            var prescription = await _api.GetAsync<PrescriptionDto>($"prescriptions/{_list.SelectedItem.Id}");
            using var form = new ObjectEditorForm(prescription, "Prescription " + prescription.PrescriptionNumber, readOnly: true);
            form.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Prescription", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ActionAsync(string action, object? body)
    {
        if (_list.SelectedItem is null) return;
        try
        {
            await _api.PostAsync<object>($"prescriptions/{_list.SelectedItem.Id}/{action}", body);
            await _list.LoadAsync();
            MessageBox.Show($"Prescription {action} completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Prescription", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
