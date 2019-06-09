using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using WpfExtensions.Xaml.ExtensionMethods;

namespace WpfExtensions.Xaml.Router
{
    public class RouteCollection : Collection<RouteBase> { }

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

        #region Attch events

        public static readonly RoutedEvent UnloadingEvent = EventManager.RegisterRoutedEvent(
            "Unloading", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(BrowserRouter));

        public static void AddUnloadingHandler(DependencyObject d, RoutedEventHandler handler)
        {
            if (d is UIElement uiElement)
            {
                uiElement.AddHandler(UnloadingEvent, handler);
            }
        }
        public static void RemoveUnloadingHandler(DependencyObject d, RoutedEventHandler handler)
        {
            if (d is UIElement uiElement)
            {
                uiElement.RemoveHandler(UnloadingEvent, handler);
            }
        }

        #endregion

        #region Static members

        private static readonly HashSet<BrowserRouter> GlobalTopRouters = new HashSet<BrowserRouter>();

        private static IReadOnlyList<string> _currentPathFragments;

        public static void Navigate(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            _currentPathFragments = path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var topRouter in GlobalTopRouters)
            {
                // Routers of the same level should be loaded at the same time.
#pragma warning disable 4014
                topRouter.NavigateAsync(0);
#pragma warning restore 4014
            }
        }

        #endregion

        private readonly Collection<BrowserRouter> _children = new Collection<BrowserRouter>();
        private BrowserRouter _parent;
        private RouteBase _currentRoute;

        public BrowserRouter()
        {
            Routes = new RouteCollection();

            Loaded += OnRouterLoaded;
            Unloaded += OnRouterUnloaded;
        }

        private async void OnRouterLoaded(object sender, RoutedEventArgs e)
        {
            _parent = this.TryFindParent<BrowserRouter>();
            _parent?.AddChild(this);
            if (_parent == null)
            {
                GlobalTopRouters.Add(this);
            }

            await NavigateAsync(GetRouterLevel());
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

        private async Task NavigateAsync(int routerLevel)
        {
            if (_currentPathFragments == null || _currentPathFragments.Count <= routerLevel || routerLevel < 0) return;

            var matchedRoute = FindRouteByPath(_currentPathFragments[routerLevel]);

            if (matchedRoute != null)
            {
                if (_currentRoute == matchedRoute)
                {
                    routerLevel++;
                    foreach (var child in _children)
                    {
                        await child.NavigateAsync(routerLevel);
                    }
                }
                else
                {
                    if (Content is UIElement oldUiElement)
                    {
                        oldUiElement.RaiseEvent(new RoutedEventArgs(UnloadingEvent, oldUiElement));
                        await Task.Delay(_currentRoute.UnloadTimeout);
                    }

                    Content = matchedRoute.Component;
                    // In order to activate bindings of this view,
                    // if not, sometimes the binding value will be `DependencyProperty.UnsetValue`.
                    // I don't know the reason for it. Can someone tell me?
                    Content = null;
                    Content = matchedRoute.Component;

                    _currentRoute = matchedRoute;
                }
            }
            else
            {
                throw new InvalidOperationException("404: NOT FOUND. ");
            }
        }

        private RouteBase FindRouteByPath(string pathFragment)
        {
            var trimPath = pathFragment.Trim(PathSeparators);
            return Routes.OfType<Route>().FirstOrDefault(item =>
                trimPath.Equals(item.Path.Trim(PathSeparators), StringComparison.InvariantCultureIgnoreCase)) ??
                (RouteBase)Routes.OfType<DefaultRoute>().FirstOrDefault();
        }

        private int GetRouterLevel()
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
