using System;
using System.Collections.Generic;

namespace WpfExtensions.Binding.Expressions
{
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
                callback?.Invoke(item);
            }
        }

        public static T? TryGet<T>(this Func<T>? getter, out Exception? exception)
        {
            try
            {
                exception = null;
                return getter != null ? getter() : default;
            }
            catch (Exception e)
            {
                exception = e;
                return default;
            }
        }
    }
}
