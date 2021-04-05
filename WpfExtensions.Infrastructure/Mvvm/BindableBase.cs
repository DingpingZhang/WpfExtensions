using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using WpfExtensions.Infrastructure.DataBinding;

namespace WpfExtensions.Infrastructure.Mvvm
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        private readonly IDictionary<string, object> _propertyValueStorage = new ConcurrentDictionary<string, object>();
        private readonly HashSet<string> _existedObserverPropertyNameHashSet = new();

        public event PropertyChangedEventHandler PropertyChanged;

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

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);

            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected sealed class PropertyObserver : IDisposable
        {
            private readonly Action _callback;
            private readonly HashSet<string> _existedExpressionHashSet = new();
            private readonly List<IDisposable> _disposables = new();
            private Func<bool> _globalCondition;

            public PropertyObserver(Action callback) => _callback = callback;

            public PropertyObserver Observes<T>(Expression<Func<T>> expression, Func<T, bool> condition = null)
            {
                var expressionString = expression.ToString();
                if (!_existedExpressionHashSet.Add(expressionString))
                {
                    throw new ArgumentException($"The expression ({expressionString}) already exists.");
                }

                var disposable = ExpressionObserver.Observes(expression, (value, _) =>
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
