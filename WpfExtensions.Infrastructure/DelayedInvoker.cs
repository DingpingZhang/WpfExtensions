using System;
using System.Threading.Tasks;

namespace WpfExtensions.Infrastructure;

public class DelayedInvoker
{
    private readonly Action _action;
    private readonly TimeSpan _delaySpan;
    private DateTimeOffset _invokeTime;
    private bool _running;
    private bool _cancelled;

    public DelayedInvoker(Action action, TimeSpan delaySpan)
    {
        _action = action;
        _delaySpan = delaySpan;
    }

    public async void Invoke()
    {
        _cancelled = false;

        Refresh();

        if (!_running) await InternalInvoke();
    }

    public void Refresh() => _invokeTime = DateTimeOffset.Now;

    public void Cancel() => _cancelled = true;

    private async Task InternalInvoke()
    {
        _running = true;

        var interval = DateTimeOffset.Now - _invokeTime;
        while (interval < _delaySpan)
        {
            if (_cancelled) break;
            await Task.Delay(_delaySpan - interval);
            interval = DateTimeOffset.Now - _invokeTime;
        }

        if (!_cancelled) _action?.Invoke();

        _running = false;
    }
}