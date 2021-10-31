using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using WpfExtensions.Binding.Expressions;

namespace WpfExtensions.Binding
{
    public delegate void RaisePropertyChangedHandler(string propertyName);

    public sealed class ComputedProperty : IDisposable
    {
        private readonly RaisePropertyChangedHandler _raisePropertyChanged;
        private readonly object _locker = new();
        private readonly Dictionary<string, IValueWrapper> _propertyValueStorage = new();
        private readonly HashSet<string> _existedObserverPropertyNameHashSet = new();
        private readonly List<IDisposable> _disposables = new();

        public ComputedProperty(RaisePropertyChangedHandler callback)
        {
            _raisePropertyChanged = callback;
        }

        public IPropertyObserver Make(string propertyName)
        {
            if (!_existedObserverPropertyNameHashSet.Add(propertyName))
            {
                throw new ArgumentException($"The property ({propertyName}) already exists.");
            }

            return new PropertyObserver(
                () => _raisePropertyChanged(propertyName),
                (expressionString, exception) => Debug.WriteLine(
                    $"[{DateTime.Now:HH:mm:ss.fff}] PropertyObserver Error: " +
                    $"propertyName = {propertyName}, " +
                    $"expressionString = {expressionString}, " +
                    $"exception = {exception}."));
        }

        public T Computed<T>(Expression<Func<T>> expression, T fallback, [CallerMemberName] string? propertyName = null)
        {
            return ComputedInternal(expression, fallback, propertyName!);
        }

        public T? Computed<T>(Expression<Func<T>> expression, [CallerMemberName] string? propertyName = null)
        {
            return ComputedInternal<T, T?>(expression, default, propertyName!);
        }

        private TOut ComputedInternal<T, TOut>(Expression<Func<T>> expression, TOut fallback, string propertyName)
            where T : TOut
        {
            lock (_locker)
            {
                if (!_propertyValueStorage.ContainsKey(propertyName))
                {
                    // The expression is first evaluated once during property initialization.
                    TOut EvaluateExpression()
                    {
                        try
                        {
                            return expression.Compile()();
                        }
                        catch
                        {
                            return fallback;
                        }
                    }

                    _propertyValueStorage.Add(propertyName, new ValueWrapper<TOut>(EvaluateExpression()));
                    IDisposable token = ExpressionObserver.Singleton.Observes(expression, (value, exception) =>
                    {
                        var storage = (ValueWrapper<TOut>)_propertyValueStorage[propertyName];
                        storage.Value = exception is null ? value : fallback;

                        // Notify ui to pull the latest value after updating the storage.
                        _raisePropertyChanged(propertyName);
                    });
                    _disposables.Add(token);
                }
            }

            return ((ValueWrapper<TOut>)_propertyValueStorage[propertyName]).Value;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }

        private interface IValueWrapper
        {
        }

        private class ValueWrapper<T> : IValueWrapper
        {
            // Avoid value types being boxed and unboxed.
            public T Value { get; set; }

            public ValueWrapper(T value) => Value = value;
        }
    }
}
