using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace WpfExtensions.Binding;

internal class PropertyGetterCache
{
    private static readonly Dictionary<Key, Func<object, object>> Cache = new();

    public static Func<object, object>? Get(Type ownerType, string propertyName)
    {
        var key = new Key(ownerType, propertyName);
        return Cache.TryGetValue(key, out var value) ? value : null;
    }

    public static Func<object, object> Get(PropertyInfo propertyInfo)
    {
        var key = new Key(propertyInfo.DeclaringType, propertyInfo.Name);
        if (!Cache.TryGetValue(key, out var getter))
        {
            getter = CreateGetter(propertyInfo);
            Cache.Add(key, getter);
        }

        return getter;
    }

    private static Func<object, object> CreateGetter(PropertyInfo info)
    {
        var args = Expression.Parameter(typeof(object), "x");
        var expr = Expression.Lambda<Func<object, object>>(
            Expression.MakeMemberAccess(Expression.Convert(args, info.DeclaringType), info),
            args
        );
        return expr.Compile();
    }

    private readonly struct Key
    {
        public readonly Type Type;
        public readonly string Name;

        public Key(Type type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}
