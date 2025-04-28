# notab

## For Users

**What it does:**

notab is a simple tool to quickly switch your Windows device between desktop mode and tablet mode.

**How to use it:**

1.  **Run as Administrator:** You MUST start this application with administrator privileges. Right-click the `.exe` file and choose "Run as administrator".
2.  **Find the Icon:** Look for the notab icon in your system tray (usually near the clock). It will look like a small desktop monitor or a tablet.
3.  **Switch Modes:** Double-click the icon. This will switch your device between desktop and tablet mode. The icon will change to show the current mode.
4.  **Exit:** Right-click the icon and select "Exit" to close the application.

**Important Notes:**

*   If you don't run it as administrator, it will show an error message and close.
*   If it encounters an error while trying to change the mode, it will show an error message and close.

## For Developers

**Core Functionality:**

*   Modifies the `ConvertibleSlateMode` DWORD value within the Windows Registry key `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\PriorityControl`.
    *   `0` = Tablet Mode
    *   `1` = Desktop Mode

**Implementation Details:**

*   Built with C# and .NET Windows Forms (`ApplicationContext` for tray-only operation).
*   Uses `NotifyIcon` to provide the system tray interface and interaction.
*   Reads the initial state from the registry on launch to display the correct icon.
*   Double-click event handler toggles the state and writes the new value to the registry using `Microsoft.Win32.Registry`.
*   Checks for administrator privileges on startup using `System.Security.Principal.WindowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator)`.

**Error Handling:**

*   Displays a `System.Windows.Forms.MessageBox` and terminates via `System.Environment.Exit(1)` upon critical errors:
    *   Lack of administrator privileges on startup.
    *   Exceptions during registry read (`Registry.GetValue`) or write (`Registry.SetValue`) operations.
