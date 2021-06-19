using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace WpfExtensions.Binding
{
    public static class ExpressionObserver
    {
        internal static DependencyGraph GenerateDependencyGraph<T>(Expression<Func<T>> expression)
        {
            var visitor = new SingleLineLambdaVisitor();
            visitor.Visit(expression);
            return new DependencyGraph(visitor.RootNodes, visitor.ConditionalNodes);
        }

        public static IDisposable Observes<T>(Expression<Func<T>> expression, Action<T, Exception> onValueChanged)
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
                onValueChanged?.Invoke(newValue, exception);

                Debug.WriteLine($"[{DateTime.Now}][Value Changed] NewValue = {newValue}");
            }
        }
    }
}
