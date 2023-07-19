using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace WpfExtensions.Binding;

internal class DependencyNode : IEquatable<DependencyNode>
{
    private readonly string _id;
    private readonly string? _propertyName;
    private readonly Func<object?>? _inpcGetter;

    public bool IsRoot { get; }

    public bool IsVirtual => !IsRoot && string.IsNullOrWhiteSpace(_propertyName) && _inpcGetter != null;

    public bool IsLeaf => !DownstreamNodes.Any();

    public ICollection<DependencyNode> DownstreamNodes { get; } = new HashSet<DependencyNode>();

    public DependencyNode(Expression node, bool isRoot = false)
    {
        _id = node.ToString();
        IsRoot = isRoot;

        if (!node.Type.IsValueType)
        {
            _inpcGetter = Expression.Lambda<Func<object?>>(node).Compile();
        }

        if (node is MemberExpression memberExpression)
        {
            _propertyName = memberExpression.Member.Name;
        }
    }

    #region Observes property changed

    private INotifyPropertyChanged? _inpcObjectCache;
    private bool _isInitialized;
    private bool _isActivated;

    public event Action? Changed;

    public bool IsActivated
    {
        get => _isActivated;
        set
        {
            // Make sure it won't be updated repeatedly.
            if (_isActivated == value)
            {
                return;
            }

            _isActivated = value;

            Unsubscribe();
            if (value)
            {
                // Update (unsubscribe and subscribe) this node,
                // because this node may be changed when it is disable.
                Subscribe();
            }
        }
    }

    public IDisposable Initialize(Action onExpressionChanged)
    {
        // Sometimes some nodes have multiple parent nodes, and do not need to be initialized repeatedly.
        if (_isInitialized)
        {
            return Disposable.Empty;
        }

        _isActivated = true;

        Changed += onExpressionChanged;
        Subscribe();

        IDisposable[] disposables = DownstreamNodes
            .Select(item => item.Initialize(onExpressionChanged))
            .ToArray();

        _isInitialized = true;

        return Disposable.Create(() =>
        {
            _isActivated = false;

            Changed -= onExpressionChanged;
            Unsubscribe();

            disposables.ForEach(item => item.Dispose());
            _isInitialized = false;
        });
    }

    private void SubscribeRecursively()
    {
        Subscribe();

        DownstreamNodes
            .Where(item => !item.IsLeaf && item.IsActivated)
            .ForEach(item => item.SubscribeRecursively());
    }

    private void UnsubscribeRecursively()
    {
        Unsubscribe();

        DownstreamNodes
            .Where(item => !item.IsLeaf && item.IsActivated)
            .ForEach(item => item.UnsubscribeRecursively());
    }

    private void Subscribe()
    {
        // Update the INPC object
        if (Suppress(_inpcGetter) is INotifyPropertyChanged inpc)
        {
            inpc.PropertyChanged += OnPropertyChanged;
            _inpcObjectCache = inpc;

            Debug.WriteLine($"[{DateTime.Now}][Bound] {this} has been bound. ");
        }

        // Local methods:
        static object? Suppress(Func<object?>? getter)
        {
            try
            {
                return getter?.Invoke();
            }
            catch (NullReferenceException)
            {
                // FIXME: The single line expression do not support `A?.B` syntactic sugar,
                // so ignore this exception here, which may cause performance problems.
                return null;
            }
        }
    }

    private void Unsubscribe()
    {
        if (_inpcObjectCache != null)
        {
            _inpcObjectCache.PropertyChanged -= OnPropertyChanged;
            _inpcObjectCache = null;

            Debug.WriteLine($"[{DateTime.Now}][Unbound] {this} has been unbound. ");
        }
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        Debug.WriteLine($"[{DateTime.Now}][Property Changed] {sender}.{e.PropertyName}");

        if (string.IsNullOrWhiteSpace(e.PropertyName))
        {
            return;
        }

        DependencyNode? changedNode = DownstreamNodes.FirstOrDefault(item => item._propertyName == e.PropertyName);
        if (changedNode is null)
        {
            return;
        }

        changedNode.UnsubscribeRecursively();
        if (changedNode.IsActivated)
        {
            changedNode.Changed?.Invoke();
            changedNode.SubscribeRecursively();
        }
    }

    #endregion

    #region Equatable memebers

    public bool Equals(DependencyNode? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || string.Equals(_id, other._id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((DependencyNode)obj);
    }

    public override int GetHashCode() => _id.GetHashCode();

    public static bool operator ==(DependencyNode left, DependencyNode right) => Equals(left, right);

    public static bool operator !=(DependencyNode left, DependencyNode right) => !Equals(left, right);

    #endregion

    public override string ToString() => IsRoot
        ? $"<Root:{_propertyName}>"
        : IsVirtual
            ? $"<Virtual>"
            : $"<{(IsLeaf ? "Leaf" : "Relay")}:{_propertyName}>";
}
