using System;
using System.Linq.Expressions;

namespace WpfExtensions.Binding;

public delegate void OnCleanup(Action cleanup);
public delegate void WatchCallback<in T>(T value, T oldValue, OnCleanup onCleanup);

public static class ReactivityExtensions
{
    public static IDisposable Watch<T>(this IReactivity self, Expression<Func<T>> expression, WatchCallback<T> callback)
    {
        Func<T> getter = expression.Compile();
        T oldValue = getter();
        Action? cleanup = null;
        void OnCleanup(Action action) => cleanup = action;
        return self.Watch(expression, () =>
        {
            cleanup?.Invoke();
            T value = getter();
            callback(value, oldValue, OnCleanup);
            oldValue = value;
        });
    }

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

    public static IDisposable Watch<T>(this IReactivity self, Expression<Func<T>> expression, Action<T> callback)
    {
        Func<T> getter = expression.Compile();
        return self.Watch(expression, () => callback(getter()));
    }
}
