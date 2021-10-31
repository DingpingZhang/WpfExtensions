using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WpfExtensions.Binding.Expressions;

namespace WpfExtensions.Binding
{
    internal class PropertyObserver : IPropertyObserver
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

            var disposable = ExpressionObserver.Singleton.Observes(expression, (value, exception) =>
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
}
