using System;
using System.Linq;
using System.Collections.Generic;

namespace WpfExtensions.Xaml.ExtensionMethods
{
    public static class EnumerableExtensions
    {
        private class EqualityComparerImpl<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> _comparer;

            public EqualityComparerImpl(Func<T, T, bool> comparer) => _comparer = comparer ?? throw new ArgumentNullException();

            public bool Equals(T x, T y) => _comparer(x, y);

            public int GetHashCode(T obj) => obj.GetHashCode();
        }

        private static readonly Dictionary<string, object> _comparerCache = new Dictionary<string, object>();

        public static IEnumerable<T> Union<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> comparer)
        {
            var key = typeof(T).FullName;
            if (!_comparerCache.TryGetValue(key, out var value))
            {
                _comparerCache.Add(key, new EqualityComparerImpl<T>(comparer));
            }

            return first.Union(second, (IEqualityComparer<T>)value);
        }
    }
}
