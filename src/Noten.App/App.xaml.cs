using System.Threading;
using WpfApplication = System.Windows.Application;
using WpfExitEventArgs = System.Windows.ExitEventArgs;
using WpfStartupEventArgs = System.Windows.StartupEventArgs;

namespace Noten.App;

public partial class App : WpfApplication
{
    private Mutex? _singleInstanceMutex;

    protected override void OnStartup(WpfStartupEventArgs e)
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

    protected override void OnExit(WpfExitEventArgs e)
    {
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
