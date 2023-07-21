using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;

namespace WpfExtensions.Binding;

/// <summary>
/// A callback delegate used to clean up the results of the last call.
/// </summary>
/// <param name="cleanup">The logic of clean up.</param>
public delegate void OnCleanup(Action cleanup);

/// <summary>
/// A callback delegate it contains an <see cref="OnCleanup"/> parameter
/// that can be used to clean up the results of the last call
/// and is generally used in asynchronous callbacks.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <param name="value">The new value after the change.</param>
/// <param name="oldValue">The old value before the change.</param>
/// <param name="onCleanup">It used to clean up the results of the last call.</param>
public delegate void WatchCallback<in T>(T value, T oldValue, OnCleanup onCleanup);

/// <summary>
/// Extension methods for <see cref="IReactivity"/> interface.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Watches an expression and invokes a callback function when the expression change.
    /// </summary>
    /// <typeparam name="T">The type of the expression.</typeparam>
    /// <param name="self">The <see cref="IReactivity"/> instance.</param>
    /// <param name="expression">The watched expression.</param>
    /// <param name="callback">
    /// A callback function it contains an <see cref="OnCleanup"/> parameter
    /// that can be used to clean up the results of the last call
    /// and is generally used in asynchronous callbacks.
    /// </param>
    /// <returns>Returns a token for unwatching.</returns>
    public static IDisposable Watch<T>(this IReactivity self, Expression<Func<T>> expression, WatchCallback<T> callback)
    {
        Action? cleanup = null;
        void OnCleanup(Action action) => cleanup = action;
        return self.Watch(expression, (value, oldValue) =>
        {
            cleanup?.Invoke();
            callback(value, oldValue, OnCleanup);
        });
    }

    /// <summary>
    /// Watches an expression and invokes a callback function when the expression change.
    /// </summary>
    /// <typeparam name="T">The type of the expression.</typeparam>
    /// <param name="self">The <see cref="IReactivity"/> instance.</param>
    /// <param name="expression">The watched expression.</param>
    /// <param name="callback">
    /// A callback function it contains two parameters,
    /// the new and old values before and after the change.
    /// </param>
    /// <returns>Returns a token for unwatching.</returns>
    public static IDisposable Watch<T>(this IReactivity self, Expression<Func<T>> expression, Action<T, T> callback)
    {
        Func<T> getter = expression.Compile();
        T oldValue = getter();
        return self.Watch(expression, () =>
        {
            T value = getter();
            callback(value, oldValue);
            oldValue = value;
        });
    }

    /// <summary>
    /// Watches an expression and invokes a callback function when the expression change.
    /// </summary>
    /// <typeparam name="T">The type of the expression.</typeparam>
    /// <param name="self">The <see cref="IReactivity"/> instance.</param>
    /// <param name="expression">The watched expression.</param>
    /// <param name="callback">
    /// A callback function it contains one parameters, the new value after the change.
    /// </param>
    /// <returns>Returns a token for unwatching.</returns>
    public static IDisposable Watch<T>(this IReactivity self, Expression<Func<T>> expression, Action<T> callback)
    {
        Func<T> getter = expression.Compile();
        return self.Watch(expression, () => callback(getter()));
    }

    /// <summary>
    /// Watches a <see cref="INotifyPropertyChanged"/> or <see cref="INotifyCollectionChanged"/> object deeply
    /// and invokes a callback function when properties in this object changed.
    /// </summary>
    /// <param name="self">The <see cref="IReactivity"/> instance.</param>
    /// <param name="target">The watched object, which must be <see cref="INotifyPropertyChanged"/> or <see cref="INotifyCollectionChanged"/> type.</param>
    /// <param name="callback">A callback function without parameters.</param>
    /// <returns>Returns a token for unwatching.</returns>
    public static IDisposable WatchDeep(this IReactivity self, object target, Action callback)
    {
        return self.WatchDeep(target, _ => callback());
    }

    /// <summary>
    /// Run a <see cref="Action"/> and collect the unwatching tokens from it into the <see cref="Scope"/>.
    /// </summary>
    /// <param name="scope">The watching scope.</param>
    /// <param name="action">The action contains a series of subscription methods.</param>
    public static void Run(this Scope scope, Action action)
    {
        using (scope.Begin())
        {
            action();
        }
    }

    internal static void ForEach<T>(this IEnumerable<T>? enumerable, Action<T> callback)
    {
        if (enumerable == null)
        {
            return;
        }

        foreach (T item in enumerable)
        {
            callback(item);
        }
    }

    internal static bool IsNotify(this Type type)
    {
        return typeof(INotifyPropertyChanged).IsAssignableFrom(type)
            || typeof(INotifyCollectionChanged).IsAssignableFrom(type);
    }
}
