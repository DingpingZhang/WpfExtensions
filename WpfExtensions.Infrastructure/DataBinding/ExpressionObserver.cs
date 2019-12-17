using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace WpfExtensions.Infrastructure.DataBinding
{
    public static class ExpressionObserver
    {
        internal static DependencyGraph GenerateDependencyGraph<T>(Expression<Func<T>> expression)
        {
            var visitor = new SingleLineLambdaVisitor();
            visitor.Visit(expression);
            return new DependencyGraph(visitor.RootNodes, visitor.ConditionalNodes);
        }

        public static void Observes<T>(Expression<Func<T>> expression, Action<T, Exception> onValueChanged)
        {
            var valueGetter = expression.Compile();

            var graph = GenerateDependencyGraph(expression);
            graph.DependencyRootNodes.ForEach(item => item.Initialize(OnPropertyChanged));
            graph.ConditionalRootNodes.ForEach(item => item.Initialize());

            void OnPropertyChanged(object sender, EventArgs e)
            {
                var newValue = valueGetter.TryGet(out var exception);
                onValueChanged?.Invoke(newValue, exception);

                Debug.WriteLine($"[{DateTime.Now}][Value Changed] NewValue = {newValue}");
            }
        }
    }
}
