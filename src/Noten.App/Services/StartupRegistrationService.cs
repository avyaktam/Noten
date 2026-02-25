using Microsoft.Win32;

namespace Noten.App.Services;

public sealed class StartupRegistrationService
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "Noten";

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
        return key?.GetValue(AppName) is string;
    }

    public void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, true)
            ?? Registry.CurrentUser.CreateSubKey(RunKey);

        if (enabled)
        {
            var exePath = Environment.ProcessPath ?? throw new InvalidOperationException("Process path unavailable.");
            key.SetValue(AppName, $"\"{exePath}\"");
            return;
        }

        key.DeleteValue(AppName, false);
    }
}
