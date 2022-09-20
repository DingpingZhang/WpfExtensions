// ReSharper disable once CheckNamespace

using System.Collections.Generic;

namespace WpfExtensions.Infrastructure.Extensions;

public static class DictionaryExtensions
{
    public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key) where TValue : new()
    {
        if (!@this.ContainsKey(key))
        {
            @this[key] = new TValue();
        }

        return @this[key];
    }

    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)
    {
        return @this.ContainsKey(key) ? @this[key] : default;
    }
}
