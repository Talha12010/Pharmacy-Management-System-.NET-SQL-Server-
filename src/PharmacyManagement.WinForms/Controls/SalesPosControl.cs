using System.ComponentModel;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Medicines;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.WinForms.Services;

namespace PharmacyManagement.WinForms.Controls;

public sealed class SalesPosControl : UserControl
{
    private readonly ApiClient _api;
    private readonly BindingList<MedicineLookupDto> _lookup = new();
    private readonly BindingList<CartItem> _cart = new();
    private readonly List<CreateSalePaymentRequest> _payments = new();
    private readonly DataGridView _lookupGrid = CreateGrid();
    private readonly DataGridView _cartGrid = CreateGrid();
    private readonly ListBox _paymentsList = new() { Dock = DockStyle.Fill };
    private readonly TextBox _search = new() { Dock = DockStyle.Fill };
    private readonly NumericUpDown _branch = CreateNumeric(1, 0, 1000);
    private readonly NumericUpDown _customer = CreateNumeric(0, 0, 1000000);
    private readonly NumericUpDown _prescription = CreateNumeric(0, 0, 1000000);
    private readonly NumericUpDown _discount = CreateNumeric(0, 0, 10000000, 2);
    private readonly NumericUpDown _paymentAmount = CreateNumeric(0, 0, 10000000, 2);
    private readonly ComboBox _paymentMethod = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _total = new() { Text = "$0.00", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(8, 76, 64) };

    public SalesPosControl(ApiClient api)
    {
        _api = api;
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(244, 247, 249);

        _branch.Value = _api.CurrentUser?.BranchId ?? 1;
        _branch.ValueChanged += async (_, _) => await SearchAsync();
        _lookupGrid.DataSource = _lookup;
        _cartGrid.DataSource = _cart;
        _lookupGrid.DataError += (_, e) => { e.ThrowException = false; };
        _cartGrid.DataError += (_, e) => { e.ThrowException = false; };
        _paymentMethod.DataSource = Enum.GetValues<PaymentMethod>();
        _paymentsList.FormattingEnabled = true;
        _paymentsList.Format += (_, e) =>
        {
            if (e.ListItem is CreateSalePaymentRequest p)
            {
                e.Value = $"{p.Method}: {p.Amount:C}";
            }
        };
        _paymentsList.DataSource = _payments;
        _cartGrid.ReadOnly = false;
        foreach (DataGridViewColumn column in _cartGrid.Columns)
        {
            column.ReadOnly = column.Name is not ("Quantity" or "DiscountAmount");
        }
        _cartGrid.CellValueChanged += (_, _) => RefreshTotal();
        _cartGrid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_cartGrid.IsCurrentCellDirty)
            {
                _cartGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };

        var searchPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(18, 10, 18, 4) };
        searchPanel.Controls.Add(new Label { Text = "Medicine", Width = 70, TextAlign = ContentAlignment.MiddleLeft });
        searchPanel.Controls.Add(CreateTextBoxHost(_search, 320));
        var searchButton = new Button { Text = "Search", Width = 100 };
        searchButton.Click += async (_, _) => await SearchAsync();
        searchPanel.Controls.Add(searchButton);
        _search.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SearchAsync();
            }
        };

        var addButton = new Button { Text = "Add to cart", Width = 120, Height = 32 };
        addButton.Click += AddToCart;
        var removeButton = new Button { Text = "Remove", Width = 100, Height = 32 };
        removeButton.Click += (_, _) => { if (_cartGrid.CurrentRow?.DataBoundItem is CartItem item) _cart.Remove(item); RefreshTotal(); };

        var topSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 260 };
        topSplit.Panel1.Controls.Add(_lookupGrid);
        topSplit.Panel1.Controls.Add(searchPanel);
        topSplit.Panel2.Controls.Add(_cartGrid);

        var cartButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        cartButtons.Controls.Add(removeButton);
        cartButtons.Controls.Add(addButton);
        topSplit.Panel2.Controls.Add(cartButtons);

        var checkout = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 190,
            ColumnCount = 8,
            RowCount = 4,
            Padding = new Padding(18, 10, 18, 12)
        };
        for (var i = 0; i < 8; i++)
        {
            checkout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
        }

        AddField(checkout, 0, 0, "Branch", _branch);
        AddField(checkout, 1, 0, "Customer ID", _customer);
        AddField(checkout, 2, 0, "Rx ID", _prescription);
        AddField(checkout, 3, 0, "Discount", _discount);
        AddField(checkout, 4, 0, "Pay method", _paymentMethod);
        AddField(checkout, 5, 0, "Pay amount", _paymentAmount);

        var addPayment = new Button { Text = "Add payment", Height = 30, Dock = DockStyle.Fill };
        addPayment.Click += AddPayment;
        checkout.Controls.Add(addPayment, 6, 0);

        var complete = new Button { Text = "Complete sale", Height = 40, Dock = DockStyle.Fill, BackColor = Color.FromArgb(11, 107, 91), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        complete.Click += async (_, _) => await CompleteSaleAsync();
        checkout.Controls.Add(complete, 7, 0);
        checkout.SetColumnSpan(complete, 1);

        checkout.Controls.Add(new Label { Text = "Payments", Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 0, 1);
        checkout.SetColumnSpan(checkout.GetControlFromPosition(0, 1)!, 2);
        checkout.Controls.Add(_paymentsList, 0, 2);
        checkout.SetColumnSpan(_paymentsList, 4);
        checkout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        checkout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
        checkout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

        var totalPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 20, 0) };
        totalPanel.Controls.Add(_total);
        totalPanel.Controls.Add(new Label { Text = "Total:", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Width = 70, TextAlign = ContentAlignment.MiddleRight });

        Controls.Add(topSplit);
        Controls.Add(totalPanel);
        Controls.Add(checkout);

        Load += async (_, _) => await SearchAsync();
    }

    private sealed class CartItem
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public int BatchId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineTotal => Math.Max(0, Quantity * UnitPrice - DiscountAmount);
    }

    private static DataGridView CreateGrid()
        => new()
        {
            AutoGenerateColumns = true,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

    private static NumericUpDown CreateNumeric(decimal value, decimal min, decimal max, int decimals = 0)
        => new()
        {
            Dock = DockStyle.Fill,
            Minimum = min,
            Maximum = max,
            DecimalPlaces = decimals,
            Value = value,
            ThousandsSeparator = true
        };

    private static Control CreateTextBoxHost(Control textBox, int width)
    {
        var panel = new Panel { Width = width, Height = 30 };
        textBox.Dock = DockStyle.Fill;
        panel.Controls.Add(textBox);
        return panel;
    }

    private static void AddField(TableLayoutPanel parent, int column, int row, string label, Control control)
    {
        parent.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 8F, FontStyle.Bold) }, column, row);
        parent.Controls.Add(control, column, row + 1);
    }

    private async Task SearchAsync()
    {
        try
        {
            var q = Uri.EscapeDataString(_search.Text.Trim());
            var data = await _api.GetAsync<IReadOnlyList<MedicineLookupDto>>($"medicines/lookup?q={q}&branchId={_branch.Value}&limit=50");
            _lookup.Clear();
            foreach (var item in data)
            {
                _lookup.Add(item);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Search", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddToCart(object? sender, EventArgs e)
    {
        if (_lookupGrid.CurrentRow?.DataBoundItem is not MedicineLookupDto medicine)
        {
            MessageBox.Show("Select a medicine first.", "Point of Sale", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var batch = medicine.Batches.FirstOrDefault(b => b.AvailableQuantity > 0 && b.ExpiryDate > DateTime.UtcNow);
        if (batch is null)
        {
            MessageBox.Show("No available non-expired batch for this medicine.", "Point of Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (medicine.RequiresPrescription && _prescription.Value <= 0)
        {
            MessageBox.Show("This medicine requires a verified prescription ID.", "Prescription required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var existing = _cart.FirstOrDefault(x => x.BatchId == batch.Id);
        if (existing is not null)
        {
            if (existing.Quantity + 1 > batch.AvailableQuantity)
            {
                MessageBox.Show("No more stock in this batch.", "Point of Sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            existing.Quantity++;
        }
        else
        {
            _cart.Add(new CartItem
            {
                MedicineId = medicine.Id,
                MedicineName = medicine.Name,
                BatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                AvailableQuantity = batch.AvailableQuantity,
                Quantity = 1,
                UnitPrice = batch.SellingPrice
            });
        }

        RefreshTotal();
    }

    private void AddPayment(object? sender, EventArgs e)
    {
        if (_paymentAmount.Value <= 0)
        {
            MessageBox.Show("Enter a payment amount.", "Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _payments.Add(new CreateSalePaymentRequest
        {
            Method = (PaymentMethod)_paymentMethod.SelectedItem!,
            Amount = _paymentAmount.Value,
            Reference = string.Empty
        });
        _paymentsList.RefreshItems();
        _paymentAmount.Value = 0;
    }

    private async Task CompleteSaleAsync()
    {
        if (_cart.Count == 0)
        {
            MessageBox.Show("Add at least one medicine to the cart.", "Point of Sale", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var totalAmount = _cart.Sum(x => x.LineTotal);
        if (_payments.Count == 0 && totalAmount > 0)
        {
            _payments.Add(new CreateSalePaymentRequest
            {
                Method = (PaymentMethod)_paymentMethod.SelectedItem!,
                Amount = totalAmount,
                Reference = string.Empty
            });
        }

        try
        {
            var request = new CreateSaleRequest
            {
                BranchId = (int)_branch.Value,
                CustomerId = _customer.Value > 0 ? (int)_customer.Value : null,
                PrescriptionId = _prescription.Value > 0 ? (int)_prescription.Value : null,
                DiscountAmount = _discount.Value,
                Notes = string.Empty,
                Items = _cart.Select(x => new CreateSaleItemRequest
                {
                    MedicineId = x.MedicineId,
                    BatchId = x.BatchId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    DiscountAmount = x.DiscountAmount
                }).ToList(),
                Payments = _payments.ToList()
            };

            var sale = await _api.PostAsync<SaleDto>("sales", request);
            _cart.Clear();
            _payments.Clear();
            _paymentsList.RefreshItems();
            RefreshTotal();
            MessageBox.Show($"Sale completed.\nInvoice: {sale.InvoiceNumber}\nTotal: {sale.TotalAmount:C}", "Sale complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await SearchAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Sale failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshTotal()
    {
        var total = _cart.Sum(x => x.LineTotal);
        _total.Text = total.ToString("C");
        var alreadyPaid = _payments.Sum(p => p.Amount);
        var remaining = Math.Max(0, total - alreadyPaid);
        if (_paymentAmount.Value == 0 || _paymentAmount.Value > remaining)
        {
            _paymentAmount.Value = remaining;
        }
    }
}

internal static class ListBoxExtensions
{
    internal static void RefreshItems(this ListBox list)
    {
        var dataSource = list.DataSource;
        list.DataSource = null;
        list.DataSource = dataSource;
    }
}
