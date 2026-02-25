using System.Windows;
using Forms = System.Windows.Forms;

namespace Noten.App.Services;

public sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon;

    public event Action? OpenClicked;
    public event Action? SettingsClicked;
    public event Action? ToggleAlwaysOnTopClicked;
    public event Action? ToggleStartupClicked;
    public event Action? ExitClicked;

    public TrayIconService()
    {
        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "Noten",
            Visible = true,
            Icon = System.Drawing.SystemIcons.Application,
            ContextMenuStrip = new Forms.ContextMenuStrip()
        };

        _notifyIcon.DoubleClick += (_, _) => OpenClicked?.Invoke();
        BuildMenu();
    }

    private void BuildMenu()
    {
        _notifyIcon.ContextMenuStrip!.Items.Add("Open Panel", null, (_, _) => OpenClicked?.Invoke());
        _notifyIcon.ContextMenuStrip.Items.Add("Settings", null, (_, _) => SettingsClicked?.Invoke());
        _notifyIcon.ContextMenuStrip.Items.Add("Toggle Always On Top", null, (_, _) => ToggleAlwaysOnTopClicked?.Invoke());
        _notifyIcon.ContextMenuStrip.Items.Add("Start with Windows", null, (_, _) => ToggleStartupClicked?.Invoke());
        _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (_, _) => ExitClicked?.Invoke());
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
