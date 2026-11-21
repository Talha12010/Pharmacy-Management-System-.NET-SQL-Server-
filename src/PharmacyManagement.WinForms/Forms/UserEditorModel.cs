using System.ComponentModel;

namespace PharmacyManagement.WinForms.Forms;

public sealed class UserEditorModel
{
    [Category("Account")]
    [Description("Login email address for the user.")]
    public string Email { get; set; } = string.Empty;

    [Category("Account")]
    [PasswordPropertyText(true)]
    [Description("Initial password (required for new users; at least 8 chars with uppercase, lowercase, digit, and special char).")]
    public string Password { get; set; } = "Admin@12345";

    [Category("Personal")]
    public string FirstName { get; set; } = string.Empty;

    [Category("Personal")]
    public string LastName { get; set; } = string.Empty;

    [Category("Personal")]
    public string? PhoneNumber { get; set; }

    [Category("Organization")]
    [Description("Branch ID the user belongs to.")]
    public int? BranchId { get; set; } = 1;

    [Category("Organization")]
    public bool IsActive { get; set; } = true;

    [Category("Security")]
    [Description("Primary system role (e.g., SuperAdmin, PharmacyAdmin, Pharmacist, Cashier, InventoryManager).")]
    public string Role { get; set; } = "Cashier";
}

