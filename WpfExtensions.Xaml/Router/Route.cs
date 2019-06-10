using System;
using System.Windows;

namespace WpfExtensions.Xaml.Router
{
    public class Route : FrameworkElement
    {
        public const string DefaultPath = "{02789585-66E4-4C31-ACB5-FEC68B48023D}";

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            "Path", typeof(string), typeof(Route), new PropertyMetadata(null, (o, args) =>
            {
                var @this = (Route)o;
                @this.TrimmedPath = @this.Path.Trim(BrowserRouter.PathSeparators);
            }));
        public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register(
            "Component", typeof(object), typeof(Route), new PropertyMetadata(default(object)));
        public static readonly DependencyProperty UnloadTimeoutProperty = DependencyProperty.Register(
            "UnloadTimeout", typeof(TimeSpan), typeof(Route), new PropertyMetadata(default(TimeSpan)));


        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

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

        internal string TrimmedPath { get; set; }

        public static bool Equals(Route route1, Route route2) =>
            EqualsPath(route1.TrimmedPath, route2.TrimmedPath);

        public static bool EqualsPath(string path1, string path2) =>
            path1?.Equals(path2, StringComparison.InvariantCultureIgnoreCase) ?? false;
    }
}
