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
    public class RouteCollection : Collection<Route>
    {
        protected override void InsertItem(int index, Route item)
        {
            if (Items.Any(existedItem => Route.Equals(existedItem, item)))
            {
                throw new InvalidOperationException($"Cannot add duplicate routing paths: {item.Path}");
            }

            base.InsertItem(index, item);
        }
    }

    [ContentProperty(nameof(Routes))]
    public class Switch : UserControl
    {
        public static readonly char[] PathSeparators = { '/', '\\' };

        public static readonly RoutedEvent UnloadingEvent = EventManager.RegisterRoutedEvent(
            "Unloading", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(Switch));

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

        public static readonly DependencyProperty RoutesProperty = DependencyProperty.Register(
            "Routes", typeof(RouteCollection), typeof(Switch), new PropertyMetadata(default(RouteCollection)));

        public RouteCollection Routes
        {
            get => (RouteCollection)GetValue(RoutesProperty);
            set => SetValue(RoutesProperty, value);
        }

        #region Static members

        private static readonly HashSet<Switch> GlobalTopRouters = new HashSet<Switch>();

        private static IReadOnlyList<string> _currentPathFragments;

        public static void To(string path)
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

        private readonly Collection<Switch> _children = new Collection<Switch>();
        private Switch _parent;
        private Route _currentRoute;

        public Switch()
        {
            Routes = new RouteCollection();

            Loaded += OnRouterLoaded;
            Unloaded += OnRouterUnloaded;
        }

        private async void OnRouterLoaded(object sender, RoutedEventArgs e)
        {
            _parent = this.TryFindParent<Switch>();
            if (_parent == null)
            {
                GlobalTopRouters.Add(this);
            }
            else
            {
                _parent._children.Add(this);
            }

            await NavigateAsync(GetRouterLevel(_parent));
        }

        private void OnRouterUnloaded(object sender, RoutedEventArgs e)
        {
            GlobalTopRouters.Remove(this);
            _parent?._children.Remove(this);
            _parent = null;
        }

        private async Task NavigateAsync(int routerLevel)
        {
            if (_currentPathFragments == null || _currentPathFragments.Count <= routerLevel || routerLevel < 0) return;

            var fragment = _currentPathFragments[routerLevel];
            var matchedRoute = FindRouteByPath(fragment);

            if (matchedRoute == null)
            {
                throw new InvalidOperationException($"Unable to find the specified route ({fragment}).");
            }

            if (ReferenceEquals(_currentRoute, matchedRoute))
            {
                routerLevel++;
                foreach (var child in _children)
                {
                    // Routers of the same level should be loaded at the same time.
#pragma warning disable 4014
                    child.NavigateAsync(routerLevel);
#pragma warning restore 4014
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

        private Route FindRouteByPath(string pathFragment)
        {
            return Routes.FirstOrDefault(item =>
                       Route.EqualsPath(item.TrimmedPath, pathFragment)) ??
                   Routes.FirstOrDefault(item =>
                       Route.EqualsPath(item.TrimmedPath, Route.Default));
        }

        private static int GetRouterLevel(Switch current)
        {
            var level = 0;
            var parent = current;
            while (parent != null)
            {
                level++;
                parent = parent._parent;
            }

            return level;
        }
    }
}
