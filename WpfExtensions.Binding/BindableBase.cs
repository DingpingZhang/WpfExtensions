using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using WpfExtensions.Binding.Expressions;

namespace WpfExtensions.Binding;

public abstract class BindableBase : INotifyPropertyChanged
{
    private interface IValueWrapper
    {
    }

    protected interface IPropertyObserver : IDisposable
    {
        IPropertyObserver Observes<T>(Expression<Func<T>> expression, Func<T, bool>? condition = null);

        void When(Func<bool> condition);
    }

    private class ValueWrapper<T> : IValueWrapper
    {
        // Avoid value types being boxed and unboxed.
        public T Value { get; set; }

        public ValueWrapper(T value) => Value = value;
    }

    private class PropertyObserver : IPropertyObserver
    {
        private readonly Action _callback;
        private readonly Action<string, Exception> _onError;
        private readonly HashSet<string> _existedExpressionHashSet = new();
        private readonly List<IDisposable> _disposables = new();

        private Func<bool>? _globalCondition;

        public PropertyObserver(Action callback, Action<string, Exception> onError)
        {
            _callback = callback;
            _onError = onError;
        }

        public IPropertyObserver Observes<T>(Expression<Func<T>> expression, Func<T, bool>? condition = null)
        {
            var expressionString = expression.ToString();
            if (!_existedExpressionHashSet.Add(expressionString))
            {
                throw new ArgumentException($"The expression ({expressionString}) already exists.");
            }

            var disposable = ExpressionObserver.Observes(expression, (value, exception) =>
            {
                if ((_globalCondition?.Invoke() ?? true) &&
                    (condition?.Invoke(value) ?? true))
                {
                    _callback();
                }

                if (exception is not null)
                {
                    _onError(expressionString, exception);
                }
            });
            _disposables.Add(disposable);

            return this;
        }

        public void When(Func<bool> condition) => _globalCondition = condition;

        public void Dispose() => _disposables.ForEach(item => item.Dispose());
    }

    private readonly IDictionary<string, IValueWrapper> _propertyValueStorage = new ConcurrentDictionary<string, IValueWrapper>();
    private readonly HashSet<string> _existedObserverPropertyNameHashSet = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected T Computed<T>(Expression<Func<T>> expression, T fallback, [CallerMemberName] string? propertyName = null)
    {
        return ComputedInternal(expression, fallback, propertyName!);
    }

    protected T? Computed<T>(Expression<Func<T>> expression, [CallerMemberName] string? propertyName = null)
    {
        return ComputedInternal<T, T?>(expression, default, propertyName!);
    }

    private TOut ComputedInternal<T, TOut>(Expression<Func<T>> expression, TOut fallback, string propertyName)
        where T : TOut
    {
        // The expression is first evaluated once during property initialization.
        TOut EvaluateExpression()
        {
            try
            {
                return expression.Compile()();
            }
            catch (Exception exception)
            {
                HandleComputedPropertyError(propertyName, exception);
                return fallback;
            }
        }

        if (!_propertyValueStorage.ContainsKey(propertyName ?? throw new ArgumentNullException(nameof(propertyName))))
        {
            _propertyValueStorage.Add(propertyName, new ValueWrapper<TOut>(EvaluateExpression()));
            ExpressionObserver.Observes(expression, (value, exception) =>
            {
                var storage = (ValueWrapper<TOut>)_propertyValueStorage[propertyName];
                if (exception is null)
                {
                    storage.Value = value;
                }
                else
                {
                    storage.Value = fallback;
                    HandleComputedPropertyError(propertyName, exception);
                }

                // Notify ui to pull the latest value after updating the storage.
                RaisePropertyChanged(propertyName);
            });
        }

        return ((ValueWrapper<TOut>)_propertyValueStorage[propertyName]).Value;
    }

    protected IPropertyObserver Make(string propertyName)
    {
        if (!_existedObserverPropertyNameHashSet.Add(propertyName))
        {
            throw new ArgumentException($"The property ({propertyName}) already exists.");
        }

        return new PropertyObserver(
            () => RaisePropertyChanged(propertyName),
            (expressionString, exception) => HandlePropertyObserverError(propertyName, expressionString, exception));
    }

    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

        storage = value;
        RaisePropertyChanged(propertyName);

        return true;
    }

    protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

        storage = value;
        onChanged();
        RaisePropertyChanged(propertyName);

        return true;
    }

    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    protected virtual void HandleComputedPropertyError(string propertyName, Exception exception)
    {
    }

    protected virtual void HandlePropertyObserverError(string propertyName, string expressionString, Exception exception)
    {
    }
}
