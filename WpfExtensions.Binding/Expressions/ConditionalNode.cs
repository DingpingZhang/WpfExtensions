﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfExtensions.Binding.Expressions;

internal class ConditionalNode
{
    [Flags]
    private enum NodeType
    {
        IfTrue = 1,
        IfFalse = 2,
        Both = 3,
        Test = 7
    }

    private readonly Func<bool> _testGetter;
    private readonly IDictionary<DependencyNode, NodeType> _allNodes = new Dictionary<DependencyNode, NodeType>();

    private IReadOnlyCollection<DependencyNode>? _ifTrueNodes;
    private IReadOnlyCollection<DependencyNode>? _ifFalseNodes;

    public bool IsRoot { get; }

    public bool IsActivated { get; set; }

    public bool IsEmpty => _allNodes.All(item => item.Value >= NodeType.Both) &&
                           IfTrueChild == null && IfFalseChild == null;

    public ConditionalNode? IfTrueChild { get; set; }

    public ConditionalNode? IfFalseChild { get; set; }

    public ConditionalNode(Func<bool> testGetter, bool isRoot = false)
    {
        IsRoot = isRoot;
        _testGetter = testGetter ?? throw new ArgumentNullException(nameof(testGetter));
    }

    public IDisposable Initialize()
    {
        var groups = _allNodes.GroupBy(item => item.Value, item => item.Key).ToArray();

        foreach (var group in groups)
        {
            switch (group.Key)
            {
                case NodeType.IfTrue:
                    _ifTrueNodes = group.ToList().AsReadOnly();
                    break;
                case NodeType.IfFalse:
                    _ifFalseNodes = group.ToList().AsReadOnly();
                    break;
                case NodeType.Test:
                    group.ForEach(item => item.Changed += OnChanged);
                    break;
            }
        }

        var ifTrueChildDisposable = IfTrueChild?.Initialize();
        var ifFalseChildDisposable = IfFalseChild?.Initialize();

        Update(true);

        return Disposable.Create(() =>
        {
            Update(false);

            _ifTrueNodes = null;
            _ifFalseNodes = null;
            groups.FirstOrDefault(item => item.Key == NodeType.Test)?
                .ForEach(item => item.Changed -= OnChanged);

            ifTrueChildDisposable?.Dispose();
            ifFalseChildDisposable?.Dispose();
        });
    }

    internal void AddAffectedNode(ConditionalNodeType type, DependencyNode node)
    {
        if (type == ConditionalNodeType.None)
            throw new InvalidOperationException($"Can not add a node that is {ConditionalNodeType.None} type to the affected node collection. ");

        // The root node is the constant node, so it will never be updated.
        if (node.IsRoot) return;

        var nodeType = GetNodeTypeFromConditionalNodeType(type);
        if (!_allNodes.ContainsKey(node))
        {
            _allNodes.Add(node, nodeType);
        }

        _allNodes[node] |= nodeType;
    }

    private NodeType GetNodeTypeFromConditionalNodeType(ConditionalNodeType type)
    {
        return type switch
        {
            ConditionalNodeType.Test => NodeType.Test,
            ConditionalNodeType.IfTrue => NodeType.IfTrue,
            ConditionalNodeType.IfFalse => NodeType.IfFalse,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private void OnChanged(object sender, EventArgs e)
    {
        if (!IsActivated) return;

        Update(true);
    }

    private void Update(bool activate)
    {
        IsActivated = activate;

        if (IsActivated)
        {
            bool testResult = _testGetter.TryGet(out var exception);

            if (exception == null)
            {
                UpdateInternal(IfTrueChild, _ifTrueNodes, testResult);
                UpdateInternal(IfFalseChild, _ifFalseNodes, !testResult);
            }
        }
        else
        {
            // Disable all nodes and child-conditional-expression recursively.
            _allNodes.Keys.ForEach(item => item.IsActivated = false);
            // The following two line only used to conditional expressions
            // that are larger than three levels of nesting.
            IfTrueChild?.Update(false);
            IfFalseChild?.Update(false);
        }
    }

    private static void UpdateInternal(ConditionalNode? child, IEnumerable<DependencyNode>? nodes, bool activate)
    {
        if (child != null)
        {
            child.Update(activate);
        }
        else
        {
            nodes.ForEach(item => item.IsActivated = activate);
        }
    }
}
