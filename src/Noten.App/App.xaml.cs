using System.Threading;
using System.Windows;

namespace Noten.App;

public partial class App : Application
{
    private Mutex? _singleInstanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        var createdNew = false;
        _singleInstanceMutex = new Mutex(true, "Noten.SingleInstance", out createdNew);

        if (!createdNew)
        {
            Shutdown();
            return;
        }

        base.OnStartup(e);

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
