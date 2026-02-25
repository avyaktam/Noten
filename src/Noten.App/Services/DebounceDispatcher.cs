using System.Windows.Threading;

namespace Noten.App.Services;

public sealed class DebounceDispatcher
{
    private DispatcherTimer? _timer;

    public void Debounce(TimeSpan interval, Action action)
    {
        _timer?.Stop();
        _timer = new DispatcherTimer { Interval = interval };
        _timer.Tick += (_, _) =>
        {
            _timer?.Stop();
            action();
        };
        _timer.Start();
    }
}
