using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WpfExtensions.Binding;

internal class SingleLineLambdaVisitor : ExpressionVisitor
{
    private const string ClosureClassName = "DisplayClass"; // Regex: <>__DisplayClass_\d+?_\d+?
    private static readonly Type InpcType = typeof(INotifyPropertyChanged);

    private readonly IDictionary<string, DependencyNode?> _nodes = new Dictionary<string, DependencyNode?>();
    private readonly List<ConditionalNode> _conditionalReferences = new();

    public IReadOnlyCollection<DependencyNode> RootNodes => _nodes.Values
        .Where(item => item is not null)
        .Select(item => item!)
        .Where(item => item.IsRoot && item.DownstreamNodes.Any())
        .ToList()
        .AsReadOnly();

    public IReadOnlyCollection<ConditionalNode> ConditionalNodes => _conditionalReferences
        .Where(item => item.IsRoot && !item.IsEmpty)
        .ToList()
        .AsReadOnly();

    // Property chain, and closure root field node.
    protected override Expression VisitMember(MemberExpression node)
    {
        DependencyNode? dependencyNode = GetOrCreateNode(node, CreateMemberNode);
        if (dependencyNode is null)
        {
            return base.VisitMember(node);
        }
        else
        {
            // New context
            Context context = _context.Clone(item => item.DownstreamNode = dependencyNode);
            return VisitInContext(() => base.VisitMember(node), context);
        }
    }

    // Root node.
    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (InpcType.IsAssignableFrom(node.Type))
        {
            GetOrCreateNode(node, CreateRootNode);
        }

        return node;
    }

    // Virtual node
    protected override Expression VisitBinary(BinaryExpression node)
    {
        // Both Left and Right are upstream nodes of this node.
        return VisitVirtualNode(() => base.VisitBinary(node), node);
    }

    // Virtual node
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Both Object and Args are upstream nodes of this node.
        return VisitVirtualNode(() => base.VisitMethodCall(node), node);
    }

    protected override Expression VisitConditional(ConditionalExpression node)
    {
        ConditionalNode testReference = CreateConditionalNode(node);
        _conditionalReferences.Add(testReference);

        DependencyNode? dependencyNode = null;
        if (InpcType.IsAssignableFrom(node.Type) && _context.DownstreamNode is not null)
        {
            // Create virtual node
            dependencyNode = GetOrCreateNode(node, CreateNode);
        }

        Context context = _context.Clone(item =>
        {
            item.DownstreamNode = dependencyNode;
            item.ConditionalNode = testReference;
            item.ConditionalNodeType = ConditionalNodeType.Test;
        });

        Expression testExpression = VisitInContext(() => Visit(node.Test), context);

        context.ConditionalNodeType = ConditionalNodeType.IfTrue;
        Expression ifTrueExpression = VisitInContext(() => Visit(node.IfTrue), context);

        context.ConditionalNodeType = ConditionalNodeType.IfFalse;
        Expression ifFalseExpression = VisitInContext(() => Visit(node.IfFalse), context);

        return node.Update(testExpression, ifTrueExpression, ifFalseExpression);
    }

    private ConditionalNode CreateConditionalNode(ConditionalExpression node)
    {
        Func<bool> test = Expression.Lambda<Func<bool>>(node.Test).Compile();
        ConditionalNode conditionalNode;
        switch (_context.ConditionalNodeType)
        {
            case ConditionalNodeType.None:
            case ConditionalNodeType.Test:
                conditionalNode = new ConditionalNode(test, true);
                break;
            case ConditionalNodeType.IfTrue:
                conditionalNode = new ConditionalNode(test);
                _context.ConditionalNode!.IfTrueChild = conditionalNode;
                break;
            case ConditionalNodeType.IfFalse:
                conditionalNode = new ConditionalNode(test);
                _context.ConditionalNode!.IfFalseChild = conditionalNode;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(node));
        }

        return conditionalNode;
    }

    private static DependencyNode? CreateMemberNode(MemberExpression node)
    {
        Expression? ownerNode = node.Expression;

        // Case: root node is static property.
        if (IsStaticRoot(node, ownerNode))
        {
            return new DependencyNode(node, isRoot: true);
        }

        // Case: resolve nested property in the chain. (Inpc -> Prop)
        // The root-node will be created in the VisitConstant method, and creates relay-node here.
        if (IsNestedNode(node, ownerNode!))
        {
            // (Inpc & Prop), (Any | Prop) -> Prop
            return new DependencyNode(node);
        }

        // WARN: changes of closure variable will not be observed.
        if (IsClosureVariable(node, ownerNode!))
        {
            if (InpcType.IsAssignableFrom(node.Type))
            {
                // Case: root node is a closure variable, like: ((<>c__DisplayClass_0_0).(Inpc)field).
                // The root-node will be created here, and the closure class (<>x__DisplayClass_X_X)
                // will be discarded in the VisitConstant method.
                return new DependencyNode(node, isRoot: true);
            }
            else
            {
                // Case: don't track variables that not implement INotifyPropertyChanged interface.
                return null;
            }
        }

        throw new NotSupportedException($"The expression of the type cannot be supported: '{node}'.");
    }

    private static DependencyNode CreateRootNode(Expression node) => new(node, isRoot: true);

    private static DependencyNode CreateNode(Expression node) => new(node);

    private static bool IsNestedNode(MemberExpression node, Expression ownerNode)
    {
        return InpcType.IsAssignableFrom(ownerNode.Type) &&
            node.Member.MemberType is MemberTypes.Property ||
            // If it is a field, it is required to be readonly,
            // because changes to the field cannot be observed.
            node.Member.MemberType is MemberTypes.Field && ((FieldInfo)node.Member).IsInitOnly;
    }

    private static bool IsClosureVariable(MemberExpression node, Expression ownerNode)
    {
        return ownerNode.NodeType == ExpressionType.Constant &&
            ownerNode.Type.Name.Contains(ClosureClassName) &&
            node.Member.MemberType == MemberTypes.Field;
    }

    private static bool IsStaticRoot(MemberExpression node, Expression? ownerNode)
    {
        return ownerNode is null &&
            node.Member.MemberType is MemberTypes.Property &&
            !((PropertyInfo)node.Member).CanWrite;
    }

    private DependencyNode? GetOrCreateNode<T>(T node, Func<T, DependencyNode?> creator)
        where T : Expression
    {
        string key = node.ToString();
        if (!_nodes.TryGetValue(key, out DependencyNode? dependencyNode))
        {
            _nodes.Add(key, dependencyNode = creator(node));
        }

        if (dependencyNode is null)
        {
            return null;
        }

        if (_context.ConditionalNodeType != ConditionalNodeType.None)
        {
            _context.ConditionalNode!.AddAffectedNode(_context.ConditionalNodeType, dependencyNode);
        }

        if (_context.DownstreamNode is not null)
        {
            dependencyNode.DownstreamNodes.Add(_context.DownstreamNode);
        }

        return dependencyNode;
    }

    #region Context manage

    private Context _context = new();

    private Expression VisitVirtualNode(Func<Expression> visitMethod, Expression node)
    {
        // Create virtual node and build context
        DependencyNode? dependencyNode = null;
        if (_context.DownstreamNode is not null)
        {
            dependencyNode = GetOrCreateNode(node, CreateNode);
        }

        return VisitInContext(visitMethod, _context.Clone(item => item.DownstreamNode = dependencyNode));
    }

    private Expression VisitInContext(Func<Expression> visitCallback, Context context)
    {
        // Push
        context.Parent = _context;
        _context = context;

        Expression expression = visitCallback();

        // Pop
        _context = _context.Parent;

        return expression;
    }

    private class Context
    {
        public Context? Parent { get; set; }

        public ConditionalNodeType ConditionalNodeType { get; set; }

        public DependencyNode? DownstreamNode { get; set; }

        public ConditionalNode? ConditionalNode { get; set; }

        public Context Clone(Action<Context> configure)
        {
            var duplicate = (Context)MemberwiseClone();
            configure(duplicate);

            return duplicate;
        }
    }

    #endregion
}
