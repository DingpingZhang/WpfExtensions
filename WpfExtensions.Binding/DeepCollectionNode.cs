using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace WpfExtensions.Binding;

internal class DeepCollectionNode : DeepNode
{
    private readonly INotifyCollectionChanged _incc;
    private readonly string _propertyPath;

    private Action<string>? _changed;

    private List<Child>? _children;

    public DeepCollectionNode(INotifyCollectionChanged incc, string propertyPath)
    {
        _incc = incc;
        _propertyPath = propertyPath;
    }

    public override void Subscribe(Action<string>? callback)
    {
        _changed = callback;
        _incc.CollectionChanged += OnCollectionChanged;

        _children = GetChildren().ToList();
        foreach (var item in _children.SelectMany(x => x.Nodes))
        {
            item.Subscribe(callback);
        }
    }

    public override void Unsubscribe()
    {
        _incc.CollectionChanged -= OnCollectionChanged;
        _changed = null;

        if (_children is null)
        {
            return;
        }

        foreach (var item in _children.SelectMany(x => x.Nodes))
        {
            item.Unsubscribe();
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        string name = GetFullName(_propertyPath, $"{e.Action}()");
        if (e.Action is NotifyCollectionChangedAction.Reset)
        {
            var callback = _changed;
            Unsubscribe();
            _changed?.Invoke(name);
            Subscribe(callback);
        }
        else
        {
            if (_children is not null && e.OldItems is not null)
            {
                foreach (var item in e.OldItems)
                {
                    var children = _children.Where(x => Equals(item, x.Item)).ToArray();
                    foreach (var child in children)
                    {
                        _children.Remove(child);
                        foreach (var node in child.Nodes)
                        {
                            node.Unsubscribe();
                        }
                    }
                }
            }

            _changed?.Invoke(name);
            if (_children is not null && e.NewItems is not null)
            {
                foreach (var item in e.NewItems)
                {
                    string path = GetFullName(_propertyPath, "Item[]");
                    var child = new Child(item, Create(item, path).ToArray());
                    foreach (var node in child.Nodes)
                    {
                        node.Subscribe(_changed);
                    }

                    _children.Add(child);
                }
            }
        }
    }

    private IEnumerable<Child> GetChildren()
    {
        if (_incc is not IEnumerable items)
        {
            yield break;
        }

        foreach (var item in items)
        {
            string path = GetFullName(_propertyPath, "Item[]");
            yield return new Child(item, Create(item, path).ToArray());
        }
    }

    private readonly struct Child
    {
        public readonly object Item;
        public readonly DeepNode[] Nodes;

        public Child(object item, DeepNode[] nodes)
        {
            Item = item;
            Nodes = nodes;
        }
    }
}
