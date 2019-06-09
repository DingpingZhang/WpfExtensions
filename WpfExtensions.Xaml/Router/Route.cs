using System;
using System.Windows;

namespace WpfExtensions.Xaml.Router
{
    public abstract class RouteBase : FrameworkElement
    {
        public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register(
            "Component", typeof(object), typeof(RouteBase), new PropertyMetadata(default(object)));
        public static readonly DependencyProperty UnloadTimeoutProperty = DependencyProperty.Register(
            "UnloadTimeout", typeof(TimeSpan), typeof(RouteBase), new PropertyMetadata(default(TimeSpan)));

        public object Component
        {
            get => GetValue(ComponentProperty);
            set => SetValue(ComponentProperty, value);
        }

        public TimeSpan UnloadTimeout
        {
            get => (TimeSpan)GetValue(UnloadTimeoutProperty);
            set => SetValue(UnloadTimeoutProperty, value);
        }
    }

    public class DefaultRoute : RouteBase { }

    public class Route : RouteBase
    {
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            "Path", typeof(string), typeof(Route), new PropertyMetadata(default(string)));

        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }
    }
}
