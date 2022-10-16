using System;
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
