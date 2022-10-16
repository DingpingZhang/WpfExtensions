using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace WpfExtensions.Binding;

public class Reactivity : IReactivity
{
    public static IReactivity Default { get; } = new Reactivity();

    public IDisposable Watch<T>(Expression<Func<T>> expression, Action callback)
    {
        DependencyGraph graph = GenerateDependencyGraph(expression);
        IDisposable[] tokens = graph.DependencyRootNodes
            .Select(item => item.Initialize(OnPropertyChanged))
            .Concat(graph.ConditionalRootNodes.Select(item => item.Initialize()))
            .ToArray();

        IDisposable token = Disposable.Create(() =>
        {
            foreach (IDisposable token in tokens)
            {
                token.Dispose();
            }
        });

        Binding.Scope.ActiveEffectScope?.AddStopToken(token);
        return token;

        void OnPropertyChanged(object sender, EventArgs e) => callback();
    }

    public Scope Scope(bool detached = false) => new(detached);

    private static DependencyGraph GenerateDependencyGraph<T>(Expression<Func<T>> expression)
    {
        var visitor = new SingleLineLambdaVisitor();
        visitor.Visit(expression);
        return new DependencyGraph(visitor.RootNodes, visitor.ConditionalNodes);
    }
}
