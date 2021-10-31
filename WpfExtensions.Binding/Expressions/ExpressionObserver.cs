using System;
using System.Linq;
using System.Linq.Expressions;

namespace WpfExtensions.Binding.Expressions
{
    public class ExpressionObserver : IExpressionObserver
    {
        public static IExpressionObserver Singleton { get; } = new ExpressionObserver();

        private ExpressionObserver() { }

        public IDisposable Observes<T>(Expression<Func<T>> expression, Action<T, Exception?> callback)
        {
            var valueGetter = expression.Compile();

            var graph = GenerateDependencyGraph(expression);
            var dependencyRootNodeDisposables = graph.DependencyRootNodes
                .Select(item => item.Initialize(OnPropertyChanged))
                .ToArray();
            var conditionalRootNodeDisposables = graph.ConditionalRootNodes
                .Select(item => item.Initialize())
                .ToArray();

            return Disposable.Create(() =>
            {
                dependencyRootNodeDisposables.ForEach(item => item.Dispose());
                conditionalRootNodeDisposables.ForEach(item => item.Dispose());
            });

            void OnPropertyChanged(object sender, EventArgs e)
            {
                var newValue = valueGetter.TryGet(out var exception);
                callback(newValue!, exception);
            }
        }

        private static DependencyGraph GenerateDependencyGraph<T>(Expression<Func<T>> expression)
        {
            var visitor = new SingleLineLambdaVisitor();
            visitor.Visit(expression);
            return new DependencyGraph(visitor.RootNodes, visitor.ConditionalNodes);
        }
    }
}
