using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace WpfExtensions.Binding;

internal class DeepObjectNode : DeepNode
{
    private static readonly Dictionary<Type, HashSet<string>> PropertyNameCache = new();

    private readonly INotifyPropertyChanged _inpc;
    private readonly Type _type;
    private readonly HashSet<string> _propertyNames;
    private readonly string _propertyPath;

    private Action<string>? _changed;
    private Dictionary<string, DeepNode[]>? _children;

    public DeepObjectNode(INotifyPropertyChanged inpc, string propertyPath)
    {
        _inpc = inpc;
        _type = _inpc.GetType();
        if (!PropertyNameCache.TryGetValue(_type, out var props))
        {
            props = new HashSet<string>(GetPropertyNames(_type));
            PropertyNameCache.Add(_type, props);
        }

        _propertyNames = props;
        _propertyPath = propertyPath;
    }

    public override void Subscribe(Action<string>? callback)
    {
        _changed = callback;
        _inpc.PropertyChanged += OnPropertyChanged;

        _children = GetChildren();
        foreach (var item in _children.Values.SelectMany(x => x))
        {
            item.Subscribe(callback);
        }
    }

    public override void Unsubscribe()
    {
        _inpc.PropertyChanged -= OnPropertyChanged;
        _changed = null;

        if (_children is null)
        {
            return;
        }

        foreach (var item in _children.Values.SelectMany(x => x))
        {
            item.Unsubscribe();
        }

        _children = null;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var oldNodes = ToDeepNode(e.PropertyName).ToArray();
        foreach (var node in oldNodes)
        {
            node.Unsubscribe();
        }

        _changed?.Invoke(GetFullName(_propertyPath, e.PropertyName));

        if (_children is not null)
        {
            var newNodes = GetChildByName(e.PropertyName);
            foreach (var node in newNodes)
            {
                node.Subscribe(_changed);
            }

            _children[e.PropertyName] = newNodes;
        }
    }

    private IEnumerable<DeepNode> ToDeepNode(string propertyName)
    {
        if (_children is not null && _children.TryGetValue(propertyName, out var changedNodes))
        {
            foreach (var item in changedNodes)
            {
                yield return item;
            }
        }
    }

    private Dictionary<string, DeepNode[]> GetChildren()
    {
        var ret = new Dictionary<string, DeepNode[]>();
        foreach (var name in _propertyNames)
        {
            ret.Add(name, GetChildByName(name));
        }

        return ret;
    }

    private DeepNode[] GetChildByName(string name)
    {
        object? value = PropertyGetterCache.Get(_type, name)?.Invoke(_inpc);
        return Create(value, GetFullName(_propertyPath, name)).ToArray();
    }

    private static IEnumerable<string> GetPropertyNames(Type type)
    {
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            bool isIndexer = IsIndexer(prop);
            if (prop.PropertyType.IsNotify() && !isIndexer)
            {
                _ = PropertyGetterCache.Get(prop);
            }

            yield return isIndexer ? "Item[]" : prop.Name;
        }
    }

    private static bool IsIndexer(PropertyInfo property)
    {
        return property.Name is "Item" && property.GetMethod.GetParameters().Length > 0;
    }
}
