using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WpfExtensions.Binding;

internal abstract class DeepNode
{
    public abstract void Subscribe(Action<string>? callback);

    public abstract void Unsubscribe();

    public static IEnumerable<DeepNode> Create(object? target, string path)
    {
        if (target is INotifyPropertyChanged inpc)
        {
            yield return new DeepObjectNode(inpc, path);
        }

        if (target is INotifyCollectionChanged incc)
        {
            yield return new DeepCollectionNode(incc, path);
        }
    }

    protected static string GetFullName(string owner, string name)
    {
        return string.IsNullOrEmpty(owner)
            ? string.IsNullOrEmpty(name)
                ? string.Empty
                : name
            : $"{owner}.{name}";
    }
}
