using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;

namespace WpfExtensions.Binding;

/// <summary>
/// Provides API for reactive feature.
/// </summary>
public interface IReactivity
{
    /// <summary>
    /// Watches an expression and invokes a callback function when the expression change.
    /// </summary>
    /// <typeparam name="T">The type of the expression.</typeparam>
    /// <param name="expression">The watched expression.</param>
    /// <param name="callback">The function to be invoked when the expression changes.</param>
    /// <returns>Returns a token for unwatching.</returns>
    IDisposable Watch<T>(Expression<Func<T>> expression, Action callback);

    /// <summary>
    /// Watches a <see cref="INotifyPropertyChanged"/> or <see cref="INotifyCollectionChanged"/> object deeply
    /// and invokes a callback function when properties in this object changed.
    /// </summary>
    /// <param name="target">The watched object, which must be <see cref="INotifyPropertyChanged"/> or <see cref="INotifyCollectionChanged"/> type.</param>
    /// <param name="callback">The function to be invoked when properties in this object changed.</param>
    /// <returns>Returns a token for unwatching.</returns>
    IDisposable WatchDeep(object target, Action<string> callback);

    /// <summary>
    /// Creates a scope object which can capture the watchers created within it
    /// so that these effects can be disposed together.
    /// </summary>
    /// <param name="detached">
    /// It indicates that a detached scope will be created,
    /// which will not be collected by its parent scope.
    /// </param>
    /// <returns>Return the created <see cref="Binding.Scope"/> instance.</returns>
    Scope Scope(bool detached = false);
}
