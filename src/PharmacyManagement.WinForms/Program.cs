using PharmacyManagement.WinForms.Configuration;
using PharmacyManagement.WinForms.Forms;
using PharmacyManagement.WinForms.Services;

namespace PharmacyManagement.WinForms;

static class Program
{
    [STAThread]
    static async Task Main()
    {
        ApplicationConfiguration.Initialize();

        var logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PharmacyManagement");
        Directory.CreateDirectory(logDirectory);
        var logPath = Path.Combine(logDirectory, "crash.log");
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            File.WriteAllText(logPath, args.ExceptionObject.ToString());
        System.Windows.Forms.Application.ThreadException += (_, args) =>
            File.WriteAllText(logPath, args.Exception.ToString());

        var config = AppConfig.Load();
        var api = new ApiClient(config);

        while (true)
        {
            if (!await api.TryRestoreSessionAsync())
            {
                using var login = new LoginForm(api, config);
                if (login.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            using var main = new MainForm(api);
            if (main.ShowDialog() != DialogResult.Cancel)
            {
                return;
            }
        }
    }
}
