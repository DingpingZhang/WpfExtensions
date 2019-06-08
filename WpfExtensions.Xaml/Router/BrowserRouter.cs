using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using WpfExtensions.Xaml.ExtensionMethods;

namespace WpfExtensions.Xaml.Router
{
    public class RouteCollection : ObservableCollection<Route> { }

    [ContentProperty(nameof(Routes))]
    public class BrowserRouter : UserControl
    {
        public static char[] PathSeparators = { '/', '\\', '.' };

        public static readonly DependencyProperty RoutesProperty = DependencyProperty.Register(
            "Routes", typeof(RouteCollection), typeof(BrowserRouter), new PropertyMetadata(default(RouteCollection)));

        public RouteCollection Routes
        {
            get => (RouteCollection)GetValue(RoutesProperty);
            set => SetValue(RoutesProperty, value);
        }

        #region Static members

        private static readonly HashSet<BrowserRouter> GlobalTopRouters = new HashSet<BrowserRouter>();

        private static IReadOnlyList<string> _currentPathFragments;

        public static void Navigate(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            _currentPathFragments = path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var topRouter in GlobalTopRouters)
            {
                topRouter.NavigateFor(0);
            }
        }

        #endregion

        private readonly Collection<BrowserRouter> _children = new Collection<BrowserRouter>();
        private BrowserRouter _parent;

        public BrowserRouter()
        {
            Routes = new RouteCollection();

            Loaded += OnRouterLoaded;
            Unloaded += OnRouterUnloaded;
        }

        private void OnRouterLoaded(object sender, RoutedEventArgs e)
        {
            _parent = this.TryFindParent<BrowserRouter>();
            _parent?.AddChild(this);
            NavigateFor(GetLevel());
            if (_parent == null)
            {
                GlobalTopRouters.Add(this);
            }
        }

        private void OnRouterUnloaded(object sender, RoutedEventArgs e)
        {
            GlobalTopRouters.Remove(this);
            _parent?._children.Remove(this);
            _parent = null;
        }

        private void AddChild(BrowserRouter router)
        {
            _children.Add(router);
        }

        private void NavigateFor(int level)
        {
            if (_currentPathFragments == null || _currentPathFragments.Count <= level || level < 0) return;

            var path = _currentPathFragments[level];

            var trimPath = path.Trim(PathSeparators);
            var matchedRoute = Routes.FirstOrDefault(item =>
                trimPath.Equals(item.Path.Trim(PathSeparators), StringComparison.InvariantCultureIgnoreCase));

            if (matchedRoute != null)
            {
                if (Content != null && Content.Equals(matchedRoute.Component) && _children.Any())
                {
                    level++;
                    foreach (var child in _children)
                    {
                        child.NavigateFor(level);
                    }
                }
                else
                {
                    Content = matchedRoute.Component;
                    // In order to activate bindings of this view,
                    // if not, sometimes the binding value will be `DependencyProperty.UnsetValue`.
                    // I don't know the reason for it. Can someone tell me?
                    Content = null;
                    Content = matchedRoute.Component;
                }
            }
            else
            {
                // TODO: Navigate to 404 page.
                MessageBox.Show("404: NOT FOUND THIS PAGE!");
            }
        }

        private int GetLevel()
        {
            var level = 0;
            var parent = _parent;
            while (parent != null)
            {
                level++;
                parent = parent._parent;
            }

            return level;
        }
    }
}
