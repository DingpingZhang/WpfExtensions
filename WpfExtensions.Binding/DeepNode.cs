using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WpfExtensions.Binding;

internal abstract class DeepNode
{
    public abstract void Subscribe(Action? callback);

    public abstract void Unsubscribe();

    public static IEnumerable<DeepNode> Create(object? target)
    {
        if (target is INotifyPropertyChanged inpc)
        {
            yield return new DeepObjectNode(inpc);
        }

        if (target is INotifyCollectionChanged incc)
        {
            yield return new DeepCollectionNode(incc);
        }
    }
}
