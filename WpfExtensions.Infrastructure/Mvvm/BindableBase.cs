using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using WpfExtensions.Infrastructure.DataBinding;

namespace WpfExtensions.Infrastructure.Mvvm
{
    public abstract class BindableBase : Prism.Mvvm.BindableBase
    {
        private readonly IDictionary<string, object> _propertyValueStorage = new ConcurrentDictionary<string, object>();
        private readonly HashSet<string> _existedObserverPropertyNameHashSet = new HashSet<string>();

        protected virtual T Computed<T>(Expression<Func<T>> expression, T fallback = default, [CallerMemberName] string propertyName = null)
        {
            if (!_propertyValueStorage.ContainsKey(propertyName ?? throw new ArgumentNullException(nameof(propertyName))))
            {
                _propertyValueStorage.Add(propertyName, fallback);
                ExpressionObserver.Observes(expression, (value, exception) =>
                {
                    _propertyValueStorage[propertyName] = exception == null ? value : fallback;
                    RaisePropertyChanged(propertyName);
                });
            }

            return (T)_propertyValueStorage[propertyName];
        }

        protected PropertyObserver Make(string propertyName)
        {
            if (!_existedObserverPropertyNameHashSet.Add(propertyName))
            {
                throw new ArgumentException($"The property ({propertyName}) already exists.");
            }

            return new PropertyObserver(() => RaisePropertyChanged(propertyName));
        }

        protected sealed class PropertyObserver : IDisposable
        {
            private readonly Action _callback;
            private readonly HashSet<string> _existedExpressionHashSet = new HashSet<string>();
            private readonly List<IDisposable> _disposables = new List<IDisposable>();
            private Func<bool> _globalCondition;

            public PropertyObserver(Action callback) => _callback = callback;

            public PropertyObserver Observes<T>(Expression<Func<T>> expression, Func<T, bool> condition = null)
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
                        _callback?.Invoke();
                    }
                });
                _disposables.Add(disposable);

                return this;
            }

            public void When(Func<bool> condition) => _globalCondition = condition;

            public void Dispose() => _disposables.ForEach(item => item.Dispose());
        }
    }
}
