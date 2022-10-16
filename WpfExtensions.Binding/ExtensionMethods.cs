using System;
using System.Collections.Generic;

namespace WpfExtensions.Binding;

internal static class ExtensionMethods
{
    public static void ForEach<T>(this IEnumerable<T>? enumerable, Action<T> callback)
    {
        if (enumerable == null)
        {
            return;
        }

        foreach (var item in enumerable)
        {
            callback(item);
        }
    }
}
