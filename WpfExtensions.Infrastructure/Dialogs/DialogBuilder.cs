using System;
using System.Collections.Concurrent;
using System.Windows;

namespace WpfExtensions.Infrastructure.Dialogs
{
    public static class DialogBuilder
    {
        private static readonly ConcurrentDictionary<object, Window> SingletonWindows = new ConcurrentDictionary<object, Window>();

        public static bool TryGetSingletonWindow<TView>(object key, out Window window)
        {
            return SingletonWindows.TryGetValue(GetSingletonKey(typeof(TView), key), out window);
        }

        public static IDialogBuilder Wrap<TView>(TView instance = null) where TView : FrameworkElement, new()
        {
            return Wrap(() => instance ?? new TView());
        }

        public static IDialogBuilder Wrap<TView>(Func<TView> factory) where TView : FrameworkElement
        {
            Guards.ThrowIfNull(factory);

            var builder = new DialogBuilderImpl();
            builder.Init(factory);
            return builder;
        }

        private static object GetSingletonKey(Type type, object key)
        {
            return key != null ? (object)(type.FullName, key) : type.FullName;
        }

        private class DialogBuilderImpl : IDialogBuilder
        {
            private Type _viewType;
            private Func<FrameworkElement> _viewBuilder;
            private Action<FrameworkElement, Window> _configure;
            private (bool IsSingleton, object Key) _singletonArgs;

            public void Init<TView>(Func<TView> viewBuilder) where TView : FrameworkElement
            {
                _viewType = typeof(TView);
                _viewBuilder = viewBuilder;
            }

            public IDialogBuilder View<TView>(Action<TView> settings) where TView : FrameworkElement
            {
                return settings == null
                    ? this
                    : Configure<TView, object, Window>((v, vm, w) => settings(v));
            }

            public IDialogBuilder ViewModel<TViewModel>(Action<TViewModel> settings)
            {
                return settings == null
                    ? this
                    : Configure<FrameworkElement, TViewModel, Window>((v, vm, w) => settings(vm));
            }

            public IDialogBuilder Window<TWindow>(Action<TWindow> settings) where TWindow : Window
            {
                return settings == null
                    ? this
                    : Configure<FrameworkElement, object, TWindow>((v, vm, w) => settings(w));
            }

            public IDialogBuilder Configure<TView, TViewModel, TWindow>(Action<TView, TViewModel, TWindow> configure)
                where TView : FrameworkElement
                where TWindow : Window
            {
                var previous = _configure;
                _configure = (view, window) =>
                {
                    previous?.Invoke(view, window);

                    if (!(view is TView tView))
                        throw new ArgumentException($"The view ({view?.GetType()}) is not the {typeof(TView)} type.");

                    if (!(window is TWindow tWindow))
                        throw new ArgumentException($"The window ({window?.GetType()}) is not the {typeof(TWindow)} type.");

                    if (view.DataContext is TViewModel viewModel)
                    {
                        configure?.Invoke(tView, viewModel, tWindow);
                    }
                };

                return this;
            }

            public IDialogBuilder AsSingleton(object singletonKey = null)
            {
                _singletonArgs = (true, singletonKey);
                return this;
            }

            public T Build<T>(Func<T> windowProvider, bool updateSingletonConfigure = false) where T : Window
            {
                Guards.ThrowIfNull(windowProvider);

                if (_singletonArgs.IsSingleton)
                {
                    var singletonKey = GetSingletonKey(_viewType);
                    if (SingletonWindows.TryGetValue(singletonKey, out var window))
                    {
                        if (updateSingletonConfigure)
                        {
                            _configure?.Invoke((FrameworkElement)window.Content, window);
                        }

                        return (T)window;
                    }

                    var newWindow = Application.Current.Dispatcher.Invoke(() =>
                    {
                        var dialog = NewDialogWindow(windowProvider);
                        dialog.Closed += (sender, args) => SingletonWindows.TryRemove(singletonKey, out _);
                        return dialog;
                    });

                    SingletonWindows.TryAdd(singletonKey, newWindow);
                    return newWindow;
                }

                return Application.Current.Dispatcher.Invoke(() => NewDialogWindow(windowProvider));
            }

            private T NewDialogWindow<T>(Func<T> windowProvider) where T : Window
            {
                var window = windowProvider();
                var view = _viewBuilder();
                window.Content = view;

                _configure?.Invoke(view, window);

                return window;
            }

            private object GetSingletonKey(Type type) => DialogBuilder.GetSingletonKey(type, _singletonArgs.Key);
        }
    }
}
