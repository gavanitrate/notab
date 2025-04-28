namespace notab;

using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;

static class Program
{
    [STAThread]
    static void Main()
    {
        EnsureAdministratorPrivileges();
        ApplicationConfiguration.Initialize();
        Application.Run(new SysTrayAppContext());
    }

    private static void EnsureAdministratorPrivileges()
    {
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!isAdmin)
            {
                MessageBox.Show(
                    "This application requires administrator privileges to run.\nPlease restart it as an administrator.",
                    "Administrator Privileges Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1); // Exit the application
            }
        }
    }
}

public class SysTrayAppContext : ApplicationContext
{
    private NotifyIcon trayIcon;
    private Icon desktopIcon;
    private Icon tabletIcon;
    private bool isDesktopActive;
    private const string RegistryKeyPath = @"SYSTEM\CurrentControlSet\Control\PriorityControl";
    private const string RegistryValueName = "ConvertibleSlateMode";

    public SysTrayAppContext()
    {
        desktopIcon = new Icon("static/desktop.ico");
        tabletIcon = new Icon("static/device-tablet.ico");

        int initialMode = ReadConvertibleSlateMode();
        isDesktopActive = (initialMode == 1);

        trayIcon = new NotifyIcon()
        {
            Icon = isDesktopActive ? desktopIcon : tabletIcon,
            Text = isDesktopActive ? "notab / {desktop}" : "notab / {tablet}",
            Visible = true
        };

        trayIcon.ContextMenuStrip = new ContextMenuStrip();
        trayIcon.ContextMenuStrip.Items.Add("Exit", null, OnExit);
        trayIcon.DoubleClick += OnTrayIconDoubleClick;
    }

    // https://learn.microsoft.com/en-us/windows-hardware/customize/desktop/unattend/microsoft-windows-gpiobuttons-convertibleslatemode
    private int ReadConvertibleSlateMode()
    {
        int defaultValue = 0; // Default to tablet mode if read fails

        try
        {
            using (RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(RegistryKeyPath))
            {
                if (baseKey != null)
                {
                    object value = baseKey.GetValue(RegistryValueName);
                    if (value is int intValue)
                    {
                        return intValue;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error reading registry: {ex.Message}", "Registry Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1); // Exit on registry read error
        }

        return defaultValue; // This line is now technically unreachable but kept for clarity/compilation
    }

    private void WriteConvertibleSlateMode(int mode)
    {
        try
        {
            using (RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(RegistryKeyPath, true)) // true for write access
            {
                if (baseKey != null)
                {
                    baseKey.SetValue(RegistryValueName, mode, RegistryValueKind.DWord);
                }
                else
                {
                    MessageBox.Show($"Error: Registry key '{RegistryKeyPath}' not found.", "Registry Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error writing registry: {ex.Message}", "Registry Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1); // Exit on registry write error
        }
    }

    private void OnTrayIconDoubleClick(object? sender, EventArgs e)
    {
        isDesktopActive = !isDesktopActive; // Toggle state first

        int modeToWrite = 0;

        if (isDesktopActive)
        {
            trayIcon.Icon = desktopIcon;
            trayIcon.Text = "notab / {desktop}";
            modeToWrite = 1;
        }
        else
        {
            trayIcon.Icon = tabletIcon;
            trayIcon.Text = "notab / {tablet}";
            modeToWrite = 0;
        }

        WriteConvertibleSlateMode(modeToWrite);
    }

    private void OnExit(object? sender, EventArgs e)
    {
        trayIcon.Visible = false;
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            desktopIcon?.Dispose();
            tabletIcon?.Dispose();
            trayIcon?.Dispose();
        }
        base.Dispose(disposing);
    }
}
