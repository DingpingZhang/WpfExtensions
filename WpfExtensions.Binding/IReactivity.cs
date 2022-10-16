using System;
using System.Linq.Expressions;

namespace WpfExtensions.Binding;

public interface IReactivity
{
    IDisposable Watch<T>(Expression<Func<T>> expression, Action callback);

    Scope Scope(bool detached = false);
}
