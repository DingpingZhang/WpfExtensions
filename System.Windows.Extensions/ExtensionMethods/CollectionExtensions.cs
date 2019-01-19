// ReSharper disable once CheckNamespace

using System.Collections.Generic;

namespace System.Windows.Extensions.ExtensionMethods
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
