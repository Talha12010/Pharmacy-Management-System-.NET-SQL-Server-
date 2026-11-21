using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Audit;
using PharmacyManagement.Application.DTOs.Catalog;
using PharmacyManagement.Application.DTOs.Customers;
using PharmacyManagement.Application.DTOs.Expenses;
using PharmacyManagement.Application.DTOs.Medicines;
using PharmacyManagement.Application.DTOs.Organization;
using PharmacyManagement.Application.DTOs.Suppliers;
using PharmacyManagement.Application.DTOs.Users;
using PharmacyManagement.WinForms.Controls;
using PharmacyManagement.WinForms.Services;

namespace PharmacyManagement.WinForms.Forms;

public sealed class MainForm : Form
{
    private readonly ApiClient _api;
    private readonly Panel _content = new() { Dock = DockStyle.Fill };
    private readonly FlowLayoutPanel _menu = new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
        Padding = new Padding(8, 10, 8, 8)
    };

    private readonly record struct Module(string Title, string Permission, Func<Control> Create, string Icon);

    public MainForm(ApiClient api)
    {
        _api = api;
        Text = "PharmaCore - Enterprise Pharmacy Management";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1180, 720);
        BackColor = Color.FromArgb(244, 247, 249);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel1,
            Panel1MinSize = 280,
            SplitterDistance = 300,
            SplitterWidth = 2,
            BackColor = Color.FromArgb(226, 232, 240)
        };

        var sidebar = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 76, 64) };
        var brand = new Label
        {
            Text = "  PharmaCore",
            Dock = DockStyle.Top,
            Height = 70,
            Font = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft
        };
        sidebar.Controls.Add(_menu);
        sidebar.Controls.Add(brand);
        split.Panel1.Controls.Add(sidebar);

        var header = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.White };
        var user = _api.CurrentUser;
        header.Controls.Add(new Label
        {
            Text = user is null ? string.Empty : $"{user.FullName}  |  {string.Join(", ", user.Roles)}",
            Dock = DockStyle.Right,
            Padding = new Padding(0, 18, 20, 0),
            ForeColor = Color.Gray,
            AutoSize = false,
            Width = 420
        });
        var signOut = new Button
        {
            Text = "Sign out",
            Dock = DockStyle.Right,
            Width = 100,
            Height = 34,
            Margin = new Padding(0, 12, 12, 12),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.FromArgb(11, 107, 91)
        };
        signOut.Click += (_, _) =>
        {
            _api.Logout();
            DialogResult = DialogResult.Cancel;
            Close();
        };
        header.Controls.Add(signOut);
        header.Controls.Add(new Label
        {
            Text = "  Operations Console",
            Dock = DockStyle.Left,
            Width = 320,
            Padding = new Padding(0, 18, 0, 0),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        });

        split.Panel2.Controls.Add(_content);
        split.Panel2.Controls.Add(header);
        Controls.Add(split);

        var first = BuildMenu();
        if (first is { } module)
        {
            Open(module);
        }
    }

    private Module? BuildMenu()
    {
        var modules = new List<Module>
        {
            new("Dashboard", Permissions.DashboardView, () => new DashboardControl(_api), "[=]"),
            new("Point of Sale", Permissions.SalesCreate, () => new SalesPosControl(_api), "[+]"),
            new("Sales", Permissions.SalesView, () => new SalesControl(_api), "[S]"),
            new("Medicines", Permissions.MedicinesView, CreateMedicinesControl, "[M]"),
            new("Categories", Permissions.MedicinesView, () => new EntityListControl<CategoryDto, CreateCategoryRequest>(_api, "catalog/categories", x => x.Id, Has(Permissions.MedicinesManage), Has(Permissions.MedicinesManage), Has(Permissions.MedicinesManage), "Medicine categories"), "[G]"),
            new("Manufacturers", Permissions.MedicinesView, () => new EntityListControl<ManufacturerDto, CreateManufacturerRequest>(_api, "catalog/manufacturers", x => x.Id, Has(Permissions.MedicinesManage), Has(Permissions.MedicinesManage), Has(Permissions.MedicinesManage), "Manufacturers"), "[F]"),
            new("Units", Permissions.MedicinesView, () => new EntityListControl<UnitDto, CreateUnitRequest>(_api, "catalog/units", x => x.Id, Has(Permissions.MedicinesManage), Has(Permissions.MedicinesManage), Has(Permissions.MedicinesManage), "Units"), "[U]"),
            new("Inventory", Permissions.InventoryView, () => new InventoryControl(_api), "[I]"),
            new("Purchasing", Permissions.PurchasesView, () => new PurchasesControl(_api), "[P]"),
            new("Suppliers", Permissions.SuppliersView, CreateSuppliersControl, "[V]"),
            new("Customers", Permissions.CustomersView, CreateCustomersControl, "[C]"),
            new("Prescriptions", Permissions.PrescriptionsView, () => new PrescriptionsControl(_api), "[R]"),
            new("Returns", Permissions.ReturnsView, () => new ReturnsControl(_api), "[<]"),
            new("Expenses", Permissions.ExpensesView, CreateExpensesControl, "[E]"),
            new("Expense Categories", Permissions.ExpensesView, () => new EntityListControl<ExpenseCategoryDto, CreateExpenseCategoryRequest>(_api, "expense-categories", x => x.Id, Has(Permissions.ExpensesManage), Has(Permissions.ExpensesManage), Has(Permissions.ExpensesManage), "Expense categories"), "[X]"),
            new("Reports", Permissions.ReportsView, () => new ReportsControl(_api), "[#]"),
            new("Employees", Permissions.EmployeesView, CreateEmployeesControl, "[H]"),
            new("Branches", Permissions.BranchesView, () => new EntityListControl<BranchDto, UpdateBranchRequest>(_api, "branches", x => x.Id, Has(Permissions.BranchesManage), Has(Permissions.BranchesManage), Has(Permissions.BranchesManage), "Branches"), "[B]"),
            new("Users", Permissions.UsersView, CreateUsersControl, "[U]"),
            new("Settings", Permissions.SettingsView, () => new SettingsControl(_api), "[*]"),
            new("Audit Logs", Permissions.AuditView, () => new EntityListControl<AuditLogDto, AuditLogDto>(_api, "audit-logs", x => x.Id, false, false, false, "Audit Logs"), "[A]")
        };

        Module? first = null;
        foreach (var module in modules.Where(m => Has(m.Permission)))
        {
            var button = new Button
            {
                Text = $"{module.Icon}  {module.Title}",
                Width = 260,
                Height = 42,
                Margin = new Padding(0, 0, 0, 3),
                Padding = new Padding(10, 0, 4, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(8, 76, 64),
                ForeColor = Color.FromArgb(203, 213, 225),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5F),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (_, _) =>
            {
                foreach (var other in _menu.Controls.OfType<Button>())
                {
                    other.BackColor = Color.FromArgb(8, 76, 64);
                }

                button.BackColor = Color.FromArgb(20, 184, 166);
                button.ForeColor = Color.White;
                Open(module);
            };
            _menu.Controls.Add(button);
            first ??= module;
        }

        return first;
    }

    private bool Has(string permission)
        => _api.CurrentUser?.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase) == true;

    private void Open(Module module)
    {
        foreach (Control control in _content.Controls.Cast<Control>().ToList())
        {
            _content.Controls.Remove(control);
            control.Dispose();
        }

        var view = module.Create();
        view.Dock = DockStyle.Fill;
        _content.Controls.Add(view);
        Text = "PharmaCore - " + module.Title;
    }

    private Control CreateMedicinesControl()
        => new EntityListControl<MedicineListItemDto, UpdateMedicineRequest>(
            _api, "medicines", x => x.Id, Has(Permissions.MedicinesManage), Has(Permissions.MedicinesManage), Has(Permissions.MedicinesManage), "Medicines")
        {
            CreateRequestFactory = () => new UpdateMedicineRequest
            {
                CategoryId = 1,
                ManufacturerId = 1,
                UnitId = 1,
                DosageForm = "Tablet",
                Strength = "500mg",
                PurchasePrice = 10m,
                SellingPrice = 15m,
                ReorderLevel = 10,
                IsActive = true
            },
            ResolveEditRequestAsync = async item =>
            {
                var detail = await _api.GetAsync<MedicineDto>($"medicines/{item.Id}");
                return ObjectMapper.Copy<MedicineDto, UpdateMedicineRequest>(detail);
            }
        };

    private Control CreateUsersControl()
    {
        var ctrl = new EntityListControl<UserDto, UserEditorModel>(
            _api, "users", x => x.Id, Has(Permissions.UsersManage), Has(Permissions.UsersManage), false, "Users")
        {
            CreateRequestFactory = () => new UserEditorModel
            {
                Role = "Cashier",
                BranchId = _api.CurrentUser?.BranchId ?? 1,
                Password = "Admin@12345"
            },
            ResolveEditRequestAsync = user => Task.FromResult(new UserEditorModel
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                BranchId = user.BranchId,
                IsActive = user.IsActive,
                Role = user.Roles.FirstOrDefault() ?? "Cashier",
                Password = string.Empty
            }),
            CustomCreateHandlerAsync = async model =>
            {
                await _api.PostAsync<object>("users", new CreateUserRequest
                {
                    Email = model.Email.Trim(),
                    Password = model.Password,
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    PhoneNumber = model.PhoneNumber?.Trim(),
                    BranchId = model.BranchId,
                    Roles = new List<string> { model.Role }
                });
                return true;
            },
            CustomEditHandlerAsync = async (user, model) =>
            {
                await _api.PutAsync<object>($"users/{user.Id}", new UpdateUserRequest
                {
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    PhoneNumber = model.PhoneNumber?.Trim(),
                    BranchId = model.BranchId,
                    IsActive = model.IsActive,
                    Roles = new List<string> { model.Role }
                });
                return true;
            }
        };

        if (Has(Permissions.UsersManage))
        {
            var resetPassword = new ToolStripButton("Reset Password", null, async (_, _) =>
            {
                if (ctrl.SelectedItem is null)
                {
                    MessageBox.Show("Select a user first.", "Reset password", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var newPassword = PromptInput("Enter new password for " + ctrl.SelectedItem.Email, "Admin@12345");
                if (string.IsNullOrWhiteSpace(newPassword)) return;

                try
                {
                    await _api.PostAsync<object>($"users/{ctrl.SelectedItem.Id}/reset-password", new ResetPasswordRequest { NewPassword = newPassword });
                    MessageBox.Show("Password reset successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });

            var toggleActive = new ToolStripButton("Toggle Active", null, async (_, _) =>
            {
                if (ctrl.SelectedItem is null)
                {
                    MessageBox.Show("Select a user first.", "Toggle active", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    await _api.PostAsync<object>($"users/{ctrl.SelectedItem.Id}/toggle-active", null);
                    await ctrl.LoadAsync();
                    MessageBox.Show("User status updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });

            ctrl.AddToolbarItems(resetPassword, toggleActive);
        }

        return ctrl;
    }

    private Control CreateExpensesControl()
        => new EntityListControl<ExpenseDto, UpdateExpenseRequest>(
            _api, "expenses", x => x.Id, Has(Permissions.ExpensesManage), Has(Permissions.ExpensesManage), Has(Permissions.ExpensesManage), "Expenses")
        {
            CreateRequestFactory = () => new UpdateExpenseRequest
            {
                BranchId = _api.CurrentUser?.BranchId ?? 1,
                ExpenseCategoryId = 1,
                ExpenseDateUtc = DateTime.Today,
                PaymentMethod = Domain.Enums.PaymentMethod.Cash
            }
        };

    private Control CreateEmployeesControl()
        => new EntityListControl<EmployeeDto, UpdateEmployeeRequest>(
            _api, "employees", x => x.Id, Has(Permissions.EmployeesManage), Has(Permissions.EmployeesManage), Has(Permissions.EmployeesManage), "Employees")
        {
            CreateRequestFactory = () => new UpdateEmployeeRequest
            {
                EmployeeCode = "EMP-" + DateTime.Now.ToString("yyMMddHHmm"),
                BranchId = _api.CurrentUser?.BranchId ?? 1,
                HireDate = DateTime.Today,
                Status = Domain.Enums.EmployeeStatus.Active,
                Position = "Staff"
            }
        };

    private Control CreateCustomersControl()
        => new EntityListControl<CustomerDto, UpdateCustomerRequest>(
            _api, "customers", x => x.Id, Has(Permissions.CustomersManage), Has(Permissions.CustomersManage), Has(Permissions.CustomersManage), "Customers")
        {
            CreateRequestFactory = () => new UpdateCustomerRequest
            {
                Code = "CUST-" + DateTime.Now.ToString("yyMMddHHmm"),
                CreditLimit = 1000m,
                IsActive = true
            }
        };

    private Control CreateSuppliersControl()
        => new EntityListControl<SupplierDto, UpdateSupplierRequest>(
            _api, "suppliers", x => x.Id, Has(Permissions.SuppliersManage), Has(Permissions.SuppliersManage), Has(Permissions.SuppliersManage), "Suppliers")
        {
            CreateRequestFactory = () => new UpdateSupplierRequest
            {
                OpeningBalance = 0m,
                IsActive = true
            }
        };

    private static string? PromptInput(string title, string defaultValue = "")
    {
        using var form = new Form
        {
            Text = title,
            Width = 440,
            Height = 160,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false,
            MaximizeBox = false
        };
        var text = new TextBox { Dock = DockStyle.Top, Margin = new Padding(12), Text = defaultValue, Height = 28 };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, Height = 36 };
        form.Controls.Add(text);
        form.Controls.Add(ok);
        form.AcceptButton = ok;
        return form.ShowDialog() == DialogResult.OK ? text.Text : null;
    }
}
