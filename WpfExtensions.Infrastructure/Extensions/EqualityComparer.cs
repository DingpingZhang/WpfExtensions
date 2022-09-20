using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WpfExtensions.Infrastructure.Extensions;

public static class EqualityComparer
{
    private static readonly ConcurrentDictionary<Type, object> EqualityComparerCache = new();

    public static IEqualityComparer<T> Get<T>(Func<T, T, bool> comparer)
    {
        var type = typeof(T);
        if (!EqualityComparerCache.ContainsKey(type))
        {
            EqualityComparerCache[type] = new EqualityComparerImpl<T>(comparer);
        }

        return (IEqualityComparer<T>)EqualityComparerCache[type];
    }

    private class EqualityComparerImpl<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _comparer;

        public EqualityComparerImpl(Func<T, T, bool> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public bool Equals(T x, T y) => _comparer(x, y);

        public int GetHashCode(T obj) => obj.GetHashCode();
    }
}
