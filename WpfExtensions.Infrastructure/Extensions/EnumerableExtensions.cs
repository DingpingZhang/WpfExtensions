// ReSharper disable once CheckNamespace

using System;
using System.Collections.Generic;

namespace WpfExtensions.Infrastructure.Extensions;

public static class EnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> @this, Action<T> callback)
    {
        if (@this == null) return;

        foreach (var item in @this)
        {
            callback?.Invoke(item);
        }
    }

    public static IEnumerable<T> Do<T>(this IEnumerable<T> @this, Action<T> callback)
    {
        if (@this == null) yield break;

        foreach (var item in @this)
        {
            callback?.Invoke(item);
            yield return item;
        }
    }
}
