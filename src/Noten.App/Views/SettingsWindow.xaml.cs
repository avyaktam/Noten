using System.Windows;
using Noten.Core.Models;

namespace Noten.App.Views;

public partial class SettingsWindow : Window
{
    public AppSettings DraftSettings { get; }

    public SettingsWindow(AppSettings settings, string dataPath)
    {
        InitializeComponent();

        DraftSettings = new AppSettings
        {
            AlwaysOnTop = settings.AlwaysOnTop,
            MinimizeToTrayOnClose = settings.MinimizeToTrayOnClose,
            StartWithWindows = settings.StartWithWindows,
            StartMinimizedToTray = settings.StartMinimizedToTray,
            ConfirmBeforeExit = settings.ConfirmBeforeExit,
            ThemeMode = settings.ThemeMode,
            Hotkey = settings.Hotkey,
            WindowHeight = settings.WindowHeight,
            WindowLeft = settings.WindowLeft,
            WindowTop = settings.WindowTop,
            WindowWidth = settings.WindowWidth,
            LastActiveTab = settings.LastActiveTab
        };

        DataContext = new SettingsWindowModel(DraftSettings, dataPath);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsWindowModel vm)
        {
            vm.ApplyTo(DraftSettings);
        }

        DialogResult = true;
    }
}

public sealed class SettingsWindowModel
{
    public bool AlwaysOnTop { get; set; }
    public bool MinimizeToTrayOnClose { get; set; }
    public bool StartWithWindows { get; set; }
    public bool StartMinimizedToTray { get; set; }
    public bool ConfirmBeforeExit { get; set; }
    public int ThemeIndex { get; set; }
    public string Hotkey { get; set; } = "Ctrl+Space";
    public string DataPath { get; }

    public SettingsWindowModel(AppSettings settings, string dataPath)
    {
        AlwaysOnTop = settings.AlwaysOnTop;
        MinimizeToTrayOnClose = settings.MinimizeToTrayOnClose;
        StartWithWindows = settings.StartWithWindows;
        StartMinimizedToTray = settings.StartMinimizedToTray;
        ConfirmBeforeExit = settings.ConfirmBeforeExit;
        ThemeIndex = (int)settings.ThemeMode;
        Hotkey = settings.Hotkey;
        DataPath = dataPath;
    }

    public void ApplyTo(AppSettings settings)
    {
        settings.AlwaysOnTop = AlwaysOnTop;
        settings.MinimizeToTrayOnClose = MinimizeToTrayOnClose;
        settings.StartWithWindows = StartWithWindows;
        settings.StartMinimizedToTray = StartMinimizedToTray;
        settings.ConfirmBeforeExit = ConfirmBeforeExit;
        settings.Hotkey = Hotkey;
        settings.ThemeMode = ThemeIndex switch
        {
            1 => ThemeMode.Light,
            2 => ThemeMode.Dark,
            _ => ThemeMode.System
        };
    }
}
