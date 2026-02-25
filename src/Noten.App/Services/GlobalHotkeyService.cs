using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Noten.Core.Models;

namespace Noten.App.Services;

public sealed class GlobalHotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private readonly Window _window;
    private HwndSource? _source;
    private readonly int _hotkeyId = 9001;
    private bool _registered;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public event Action? HotkeyPressed;

    public GlobalHotkeyService(Window window)
    {
        _window = window;
    }

    public bool Register(HotkeyBinding binding)
    {
        Unregister();

        var handle = new WindowInteropHelper(_window).EnsureHandle();
        _source ??= HwndSource.FromHwnd(handle);
        _source?.AddHook(WndProc);

        _registered = RegisterHotKey(handle, _hotkeyId, (uint)binding.Modifiers, binding.VirtualKey);
        return _registered;
    }

    public void Unregister()
    {
        if (!_registered)
        {
            return;
        }

        var handle = new WindowInteropHelper(_window).Handle;
        if (handle != IntPtr.Zero)
        {
            UnregisterHotKey(handle, _hotkeyId);
        }

        _registered = false;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == _hotkeyId)
        {
            HotkeyPressed?.Invoke();
            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Unregister();
        _source?.RemoveHook(WndProc);
    }
}
