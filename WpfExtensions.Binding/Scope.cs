using System;
using System.Collections.Generic;

namespace WpfExtensions.Binding;

/// <summary>
/// Represents a scope which can capture the watchers created within it
/// so that these effects can be disposed together.
/// </summary>
public sealed class Scope : IDisposable
{
    internal static Scope? ActiveEffectScope { get; set; }

    private readonly List<IDisposable> _stopTokens = new();

    // Only assigned by undetached scope.
    private readonly Scope? _parent;

    // Record undetached scopes.
    private List<Scope>? _scopes;
    private Collector? _collector;

    /// <summary>
    /// An event that will be triggered when the scope is disposed.
    /// </summary>
    public EventHandler? Disposed;

    /// <summary>
    /// Gets a <see cref="bool"/> value to indicate whether the scope has been disposed.
    /// </summary>
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

    /// <summary>
    /// Begin collecting watchers.
    /// </summary>
    /// <returns>
    /// Returns an <see cref="IDisposable"/> instance that will stop this collecting behavior
    /// of the scope when it is disposed.
    /// </returns>
    /// <exception cref="ObjectDisposedException">If the scope has been disposed.</exception>
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
