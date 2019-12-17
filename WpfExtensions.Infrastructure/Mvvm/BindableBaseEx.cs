using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Prism.Mvvm;
using WpfExtensions.Infrastructure.DataBinding;

namespace WpfExtensions.Infrastructure.Mvvm
{
    public abstract class BindableBaseEx : BindableBase
    {
        private readonly IDictionary<string, object> _propertyValueStorage = new ConcurrentDictionary<string, object>();

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
            return new PropertyObserver(() => RaisePropertyChanged(propertyName));
        }

        protected class PropertyObserver
        {
            private readonly Action _callback;
            private Func<bool> _globalCondition;

            public PropertyObserver(Action callback) => _callback = callback;

            public PropertyObserver Observes<T>(Expression<Func<T>> expression, Func<T, bool> condition = null)
            {
                ExpressionObserver.Observes(expression, (value, exception) =>
                {
                    if ((_globalCondition?.Invoke() ?? true) &&
                        (condition?.Invoke(value) ?? true))
                    {
                        _callback?.Invoke();
                    }
                });

                return this;
            }

            public void When(Func<bool> condition) => _globalCondition = condition;
        }
    }
}
