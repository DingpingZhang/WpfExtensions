using System;
using System.Collections.Generic;

namespace WpfExtensions.Binding;

public sealed class Scope : IDisposable
{
    internal static Scope? ActiveEffectScope { get; set; }

    private readonly List<IDisposable> _stopTokens = new();

    // Only assigned by undetached scope.
    private readonly Scope? _parent;

    // Record undetached scopes.
    private List<Scope>? _scopes;
    private Collector? _collector;

    public EventHandler? Disposed;

    public bool IsDisposed { get; private set; }

    internal Scope(bool detached)
    {
        Scope? activeScope = ActiveEffectScope;
        if (detached || activeScope is null)
        {
            return;
        }

        _parent = ActiveEffectScope;
        activeScope._scopes ??= new List<Scope>();
        activeScope._scopes.Add(this);
    }

    public IDisposable Begin()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(Scope));
        }

        _collector ??= new Collector(this);
        _collector.Reset();
        return _collector;
    }

    /// <inheritdoc/>
    public void Dispose() => Stop(fromParent: false);

    internal void AddStopToken(IDisposable token)
    {
        if (!IsDisposed)
        {
            _stopTokens.Add(token);
        }
    }

    private void Stop(bool fromParent)
    {
        if (IsDisposed)
        {
            return;
        }

        foreach (IDisposable token in _stopTokens)
        {
            token.Dispose();
        }

        OnDisposed();

        if (_scopes is not null)
        {
            foreach (Scope scope in _scopes)
            {
                scope.Stop(fromParent: true);
            }
        }

        // Nested scope, dereference from parent to avoid memory leaks.
        if (_parent?._scopes is not null && !fromParent)
        {
            _parent._scopes.Remove(this);
        }

        IsDisposed = true;
    }

    private void OnDisposed() => Disposed?.Invoke(this, EventArgs.Empty);

    private sealed class Collector : IDisposable
    {
        private readonly Scope _owner;
        private Scope? _currentEffectScope;

        public Collector(Scope owner) => _owner = owner;

        public void Reset()
        {
            _currentEffectScope = ActiveEffectScope;
            ActiveEffectScope = _owner;
        }

        void IDisposable.Dispose() => ActiveEffectScope = _currentEffectScope;
    }
}
