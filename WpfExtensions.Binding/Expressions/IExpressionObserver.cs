using System;
using System.Linq.Expressions;

namespace WpfExtensions.Binding.Expressions
{
    public interface IExpressionObserver
    {
        IDisposable Observes<T>(Expression<Func<T>> expression, Action<T, Exception?> callback);
    }
}
