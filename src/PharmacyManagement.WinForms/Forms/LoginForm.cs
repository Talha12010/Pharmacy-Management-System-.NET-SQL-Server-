using PharmacyManagement.WinForms.Configuration;
using PharmacyManagement.WinForms.Services;

namespace PharmacyManagement.WinForms.Forms;

public sealed class LoginForm : Form
{
    private readonly ApiClient _api;
    private readonly TextBox _email = new() { Text = "admin@pharmacy.local" };
    private readonly TextBox _password = new() { Text = "Admin@12345", UseSystemPasswordChar = true };
    private readonly CheckBox _remember = new() { Text = "Remember session", Checked = true };
    private readonly Button _login = new() { Text = "Sign in" };

    public LoginForm(ApiClient api, AppConfig config)
    {
        _api = api;
        Text = "PharmaCore - Sign in";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(460, 360);
        BackColor = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(36, 28, 36, 24)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 7; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 0 ? 70 : 46));
        }

        var brand = new Label
        {
            Text = "PharmaCore\nEnterprise Pharmacy Management",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(11, 107, 91),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        layout.Controls.Add(brand, 0, 0);
        layout.SetColumnSpan(brand, 2);

        layout.Controls.Add(new Label { Text = "API", TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.Gray }, 0, 1);
        layout.Controls.Add(new Label { Text = config.ApiBaseUrl, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.Gray }, 1, 1);
        layout.Controls.Add(new Label { Text = "Email", TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        layout.Controls.Add(_email, 1, 2);
        layout.Controls.Add(new Label { Text = "Password", TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        layout.Controls.Add(_password, 1, 3);
        layout.Controls.Add(new Label { Text = string.Empty }, 0, 4);
        layout.Controls.Add(_remember, 1, 4);

        _login.Height = 40;
        _login.BackColor = Color.FromArgb(11, 107, 91);
        _login.ForeColor = Color.White;
        _login.FlatStyle = FlatStyle.Flat;
        _login.Click += Login_Click;
        layout.Controls.Add(_login, 1, 5);

        var cancel = new Button { Text = "Exit", Height = 40, DialogResult = DialogResult.Cancel };
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        layout.Controls.Add(cancel, 1, 6);

        Controls.Add(layout);
        AcceptButton = _login;
        CancelButton = cancel;
        ActiveControl = _email;
    }

    private async void Login_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_email.Text) || string.IsNullOrWhiteSpace(_password.Text))
        {
            MessageBox.Show("Enter both email and password.", "Sign in", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _login.Enabled = false;
            Cursor = Cursors.WaitCursor;
            await _api.LoginAsync(_email.Text.Trim(), _password.Text, _remember.Checked);
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Sign in failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _login.Enabled = true;
            Cursor = Cursors.Default;
        }
    }
}
