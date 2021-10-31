using System;
using System.Linq.Expressions;

namespace WpfExtensions.Binding
{
    public interface IPropertyObserver : IDisposable
    {
        IPropertyObserver Observes<T>(Expression<Func<T>> expression, Func<T, bool>? condition = null);

        void When(Func<bool> condition);
    }
}
