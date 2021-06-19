using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace WpfExtensions.Binding
{
    internal class DependencyNode : IEquatable<DependencyNode>
    {
        public string Id { get; }

        public string PropertyName { get; }

        public bool IsRoot { get; }

        public bool IsVirtual => !IsRoot && string.IsNullOrWhiteSpace(PropertyName) && InpcGetter != null;

        public bool IsLeaf => !DownstreamNodes.Any();

        public ICollection<DependencyNode> DownstreamNodes { get; } = new HashSet<DependencyNode>();

        public Func<INotifyPropertyChanged> InpcGetter { get; }

        public DependencyNode(Expression node, bool isRoot = false)
        {
            Id = node.ToString();
            IsRoot = isRoot;

            if (typeof(INotifyPropertyChanged).IsAssignableFrom(node.Type))
            {
                InpcGetter = Expression.Lambda<Func<INotifyPropertyChanged>>(node).Compile();
            }

            if (node is MemberExpression memberExpression)
            {
                PropertyName = memberExpression.Member.Name;
            }
        }

        #region Observes property changed

        private INotifyPropertyChanged _inpcObjectCache;
        private bool _isInitialized;
        private bool _isActivated;

        public event EventHandler Changed;

        public bool IsActivated
        {
            get => _isActivated;
            set
            {
                // Make sure it won't be updated repeatedly.
                if (_isActivated == value) return;
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

        public IDisposable Initialize(EventHandler onExpressionChanged)
        {
            // Sometimes some nodes have multiple parent nodes, and do not need to be initialized repeatedly.
            if (_isInitialized) return Disposable.Empty;

            _isActivated = true;

            Changed += onExpressionChanged;
            Subscribe();

            var disposables = DownstreamNodes
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
            if (InpcGetter != null)
            {
                _inpcObjectCache = InpcGetter.TryGet(out _);
                if (_inpcObjectCache == null) return;

                _inpcObjectCache.PropertyChanged += OnPropertyChanged;

                Debug.WriteLine($"[{DateTime.Now}][Bound] {this} has been bound. ");
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
            Debug.WriteLine($"[{DateTime.Now}][Property Changed] {sender}.{e?.PropertyName}");

            if (string.IsNullOrWhiteSpace(e?.PropertyName)) return;

            var changedNode = DownstreamNodes.FirstOrDefault(item => item.PropertyName == e.PropertyName);

            if (changedNode == null) return;

            changedNode.UnsubscribeRecursively();
            if (changedNode.IsActivated)
            {
                changedNode.RaiseChanged();
                changedNode.SubscribeRecursively();
            }
        }

        #endregion

        #region Equatable memebers

        public bool Equals(DependencyNode other)
        {
            if (other is null) return false;
            return ReferenceEquals(this, other) || string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((DependencyNode)obj);
        }

        public override int GetHashCode() => Id != null ? Id.GetHashCode() : 0;

        public static bool operator ==(DependencyNode left, DependencyNode right) => Equals(left, right);

        public static bool operator !=(DependencyNode left, DependencyNode right) => !Equals(left, right);

        #endregion

        public override string ToString() => IsRoot
            ? $"<Root:{GetHashCode()}>"
            : IsVirtual
                ? $"<Virtual:{GetHashCode()}>"
                : $"<{(IsLeaf ? "Leaf" : "Relay")}:{PropertyName}:{GetHashCode()}>";

        public virtual void RaiseChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
