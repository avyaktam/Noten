namespace Noten.Core.Models;

public enum ThemeMode
{
    System,
    Light,
    Dark
}

public sealed class AppSettings
{
    public bool AlwaysOnTop { get; set; }
    public bool MinimizeToTrayOnClose { get; set; } = true;
    public bool StartWithWindows { get; set; }
    public bool StartMinimizedToTray { get; set; }
    public bool ConfirmBeforeExit { get; set; } = true;
    public ThemeMode ThemeMode { get; set; } = ThemeMode.System;
    public string Hotkey { get; set; } = "Ctrl+Space";
    public double WindowTop { get; set; } = 100;
    public double WindowLeft { get; set; } = 100;
    public double WindowWidth { get; set; } = 960;
    public double WindowHeight { get; set; } = 640;
    public string LastActiveTab { get; set; } = "Notes";
}
