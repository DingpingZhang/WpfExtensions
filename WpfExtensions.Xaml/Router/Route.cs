using System.Windows;

namespace WpfExtensions.Xaml.Router
{
    public class Route : FrameworkElement
    {
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            "Path", typeof(string), typeof(Route), new PropertyMetadata(default(string)));

        public string Path
        {
            get => (string) GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register(
            "Component", typeof(object), typeof(Route), new PropertyMetadata(default(object)));

        public object Component
        {
            get => GetValue(ComponentProperty);
            set => SetValue(ComponentProperty, value);
        }
    }
}
