using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Noten.App.Services;
using Noten.App.ViewModels;
using Noten.App.Views;
using Noten.Core.Models;
using Noten.Core.Services;

namespace Noten.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly GlobalHotkeyService _hotkeyService;
    private readonly TrayIconService _tray;
    private readonly SettingsRepository _settingsRepository;
    private readonly StartupRegistrationService _startupService;

    private readonly string _appDir;
    private AppSettings _settings = new();
    private bool _isExitRequested;

    public MainWindow()
    {
        InitializeComponent();

        var executableDirectory = AppContext.BaseDirectory;
        _appDir = DataPathResolver.ResolveAppDataDirectory(
            executableDirectory,
            () => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        Directory.CreateDirectory(_appDir);

        _vm = new MainViewModel(Path.Combine(_appDir, "appdata.json"));
        _settingsRepository = new SettingsRepository(Path.Combine(_appDir, "settings.json"));
        _startupService = new StartupRegistrationService();
        DataContext = _vm;

        _hotkeyService = new GlobalHotkeyService(this);
        _hotkeyService.HotkeyPressed += ToggleVisible;

        _vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.ActiveProject))
            {
                _vm.LoadDocumentTo(NotesEditor.Document);
                _vm.RefreshVisibleSchedule();
            }
        };

        _vm.SearchResults.CollectionChanged += SearchResults_CollectionChanged;

        _tray = new TrayIconService();
        _tray.OpenClicked += ShowFromTray;
        _tray.SettingsClicked += OpenSettings;
        _tray.ToggleAlwaysOnTopClicked += () =>
        {
            _settings.AlwaysOnTop = !_settings.AlwaysOnTop;
            Topmost = _settings.AlwaysOnTop;
            PersistSettings();
        };
        _tray.ToggleStartupClicked += ToggleStartup;
        _tray.ExitClicked += ExitApplication;

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        LocationChanged += (_, _) => PersistWindowStateDebounced();
        SizeChanged += (_, _) => PersistWindowStateDebounced();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _settings = await _settingsRepository.LoadAsync();
        _settings.StartWithWindows = _startupService.IsEnabled();

        await _vm.InitializeAsync();
        _vm.Settings = _settings;
        ApplyTheme(_settings.ThemeMode);
        ApplyWindowState(_settings);
        _vm.LoadDocumentTo(NotesEditor.Document);
        _vm.RefreshVisibleSchedule();
        UpdateSearchResultsVisibility();

        RegisterConfiguredHotkey(showError: true);

        if (_settings.StartMinimizedToTray)
        {
            Hide();
        }
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (!_isExitRequested && _settings.MinimizeToTrayOnClose)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        PersistWindowState();
        PersistSettings();
        _vm.SearchResults.CollectionChanged -= SearchResults_CollectionChanged;
        _tray.Dispose();
        _hotkeyService.Dispose();
    }

    private void ExitApplication()
    {
        if (_settings.ConfirmBeforeExit)
        {
            var confirmed = MessageBox.Show("Exit Noten?", "Confirm exit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            if (!confirmed)
            {
                return;
            }
        }

        _isExitRequested = true;
        Close();
    }

    private void ToggleVisible()
    {
        if (IsVisible)
        {
            Hide();
            return;
        }

        ShowFromTray();
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        Topmost = _settings.AlwaysOnTop;
    }

    private void OpenSettings()
    {
        var dialog = new SettingsWindow(_settings, _appDir) { Owner = this };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        _settings = dialog.DraftSettings;
        _vm.Settings = _settings;
        Topmost = _settings.AlwaysOnTop;
        ApplyTheme(_settings.ThemeMode);

        RegisterConfiguredHotkey(showError: true);

        try
        {
            _startupService.SetEnabled(_settings.StartWithWindows);
        }
        catch (Exception ex)
        {
            _settings.StartWithWindows = _startupService.IsEnabled();
            MessageBox.Show($"Failed to update startup setting: {ex.Message}", "Noten", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        PersistSettings();
    }

    private void ToggleStartup()
    {
        _settings.StartWithWindows = !_settings.StartWithWindows;
        try
        {
            _startupService.SetEnabled(_settings.StartWithWindows);
            PersistSettings();
        }
        catch (Exception ex)
        {
            _settings.StartWithWindows = !_settings.StartWithWindows;
            MessageBox.Show($"Unable to change startup registration: {ex.Message}", "Noten", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void TopHoverZone_OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        => TabRevealHost.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(150)));

    private void TopHoverZone_OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        => TabRevealHost.BeginAnimation(OpacityProperty, new DoubleAnimation(0.12, TimeSpan.FromMilliseconds(220)));

    private void NotesEditor_OnTextChanged(object sender, TextChangedEventArgs e) => _vm.UpdateNotesFromDocument(NotesEditor.Document);

    private void AutosaveField_OnLostFocus(object sender, RoutedEventArgs e)
    {
        _vm.SaveSoon();
        _vm.RefreshVisibleSchedule();
    }

    private void AutosaveField_OnValueChanged(object sender, RoutedEventArgs e) => _vm.SaveSoon();

    private void ScheduleGrid_OnRowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        _vm.SaveSoon();
        _vm.RefreshVisibleSchedule();
    }

    private void ScheduleFilterCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ScheduleFilterCombo.SelectedItem is not ComboBoxItem { Tag: string tag })
        {
            return;
        }

        if (Enum.TryParse<ScheduleFilter>(tag, ignoreCase: true, out var filter))
        {
            _vm.SelectedScheduleFilter = filter;
        }
    }

    private void RenameProject_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.ActiveProject is null)
        {
            return;
        }

        var prompt = new PromptWindow("Rename project", "Project name", _vm.ActiveProject.Name) { Owner = this };
        if (prompt.ShowDialog() != true)
        {
            return;
        }

        if (!_vm.RenameActiveProject(prompt.Value))
        {
            MessageBox.Show("Project name cannot be empty.", "Noten", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void DeleteProject_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.ActiveProject is null)
        {
            return;
        }

        var name = _vm.ActiveProject.Name;
        var confirmed = MessageBox.Show($"Delete project '{name}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        if (!confirmed)
        {
            return;
        }

        _vm.DeleteActiveProject();
        _vm.LoadDocumentTo(NotesEditor.Document);
        _vm.RefreshVisibleSchedule();
        await _vm.SaveAsync();
    }

    private async void ExportProject_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.ActiveProject is null)
        {
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Noten Project (*.json)|*.json",
            FileName = $"{_vm.ActiveProject.Name}.json"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        await _vm.ExportProjectAsync(dialog.FileName);
    }

    private async void ImportProject_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Noten Project (*.json)|*.json" };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var result = await _vm.ImportProjectAsync(dialog.FileName);
            if (result.Conflict == ProjectImportConflict.IdConflict)
            {
                var replace = MessageBox.Show("Project id conflict. Replace existing? (No = import as copy)", "Import", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                _vm.ResolveImportConflict(result.Project, replace);
            }

            _vm.LoadDocumentTo(NotesEditor.Document);
            _vm.RefreshVisibleSchedule();
            await _vm.SaveAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Import failed: {ex.Message}", "Import error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }



    private void SearchResults_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateSearchResultsVisibility();
    }

    private void UpdateSearchResultsVisibility()
    {
        SearchResultsHost.Visibility = _vm.SearchResults.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Settings_Click(object sender, RoutedEventArgs e) => OpenSettings();

    private void PersistWindowStateDebounced()
    {
        _settings.WindowTop = Top;
        _settings.WindowLeft = Left;
        _settings.WindowWidth = Width;
        _settings.WindowHeight = Height;
        _settings.LastActiveTab = MainTabs.SelectedIndex switch { 0 => "Notes", 1 => "Lists", _ => "Schedule" };
    }

    private void PersistWindowState()
    {
        PersistWindowStateDebounced();
        _settings.AlwaysOnTop = Topmost;
    }

    private void PersistSettings() => _settingsRepository.SaveAsync(_settings).GetAwaiter().GetResult();

    private void ApplyWindowState(AppSettings settings)
    {
        var placement = WindowPlacementService.Clamp(
            new WindowPlacement(settings.WindowLeft, settings.WindowTop, settings.WindowWidth, settings.WindowHeight),
            new ScreenBounds(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight),
            MinWidth,
            MinHeight);

        Width = placement.Width;
        Height = placement.Height;
        Left = placement.Left;
        Top = placement.Top;

        Topmost = settings.AlwaysOnTop;
        MainTabs.SelectedIndex = settings.LastActiveTab switch { "Lists" => 1, "Schedule" => 2, _ => 0 };
    }

    private void ApplyTheme(ThemeMode mode)
    {
        var isDark = mode is ThemeMode.Dark || mode is ThemeMode.System;
        Background = isDark
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121316"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F4F8"));
        Foreground = isDark ? Brushes.WhiteSmoke : Brushes.Black;
    }

    private void RegisterConfiguredHotkey(bool showError)
    {
        if (!HotkeyParser.TryParse(_settings.Hotkey, out var binding) || !_hotkeyService.Register(binding))
        {
            if (showError)
            {
                MessageBox.Show("Hotkey registration failed. Try another combination.", "Hotkey", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void InsertLink_Click(object sender, RoutedEventArgs e)
    {
        var selected = new TextRange(NotesEditor.Selection.Start, NotesEditor.Selection.End);
        var text = selected.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var candidate = text.Trim();
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            uri = new Uri("https://example.com");
        }

        var hyperlink = new Hyperlink(NotesEditor.Selection.Start, NotesEditor.Selection.End)
        {
            NavigateUri = uri
        };
        hyperlink.Inlines.Clear();
        hyperlink.Inlines.Add(text.Trim());
    }
}
