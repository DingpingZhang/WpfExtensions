using System;

namespace WpfExtensions.Binding;

internal sealed class Disposable : IDisposable
{
    public static readonly IDisposable Empty = Create(() => { });

    public static IDisposable Create(Action action) => new Disposable(action);

    private readonly Action _action;

    private Disposable(Action action) => _action = action;

    public void Dispose() => _action();
}
