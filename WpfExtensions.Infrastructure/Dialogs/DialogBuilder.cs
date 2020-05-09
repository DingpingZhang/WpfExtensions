using System;
using System.Collections.Concurrent;
using System.Windows;

namespace WpfExtensions.Infrastructure.Dialogs
{
    public static class DialogBuilder
    {
        private static readonly ConcurrentDictionary<object, Window> SingletonWindows = new ConcurrentDictionary<object, Window>();

        public static bool Exists<TView>(object key)
        {
            return SingletonWindows.ContainsKey(GetSingletonKey(typeof(TView), key));
        }

        public static bool Exists<TView>(object key, out TView view)
        {
            var singletonKey = GetSingletonKey(typeof(TView), key);
            var exists = SingletonWindows.ContainsKey(singletonKey);

            view = exists ? (TView)SingletonWindows[singletonKey].Content : default;

            return exists;
        }

        public static IDialogBuilder<TView> Wrap<TView>(TView instance = null) where TView : FrameworkElement, new()
        {
            return new DialogBuilderImpl<TView>(instance);
        }

        private static object GetSingletonKey(Type type, object key)
        {
            return key != null ? (object)(type.FullName, key) : type.FullName;
        }

        public static Window Build<TView>(this IDialogBuilder<TView> @this) where TView : FrameworkElement =>
            @this.Build<Window>();

        private class DialogBuilderImpl<TView> : IDialogBuilder<TView>
            where TView : FrameworkElement, new()
        {
            private Func<TView> _viewBuilder;
            private Action<TView> _viewModelInitializer;
            private Action<Window> _windowSettings;
            private (bool IsSingleton, object Key) _singletonArgs;

            public DialogBuilderImpl(TView instance = null)
            {
                _viewBuilder = () => instance ?? new TView();
            }

            public IDialogBuilder<TView> View(Action<TView> settings)
            {
                var previousAction = _viewBuilder;
                _viewBuilder = () =>
                {
                    var result = previousAction?.Invoke();
                    settings?.Invoke(result);
                    return result;
                };

                return this;
            }

            public IDialogBuilder<TView> ViewModel<TViewModel>(Action<TViewModel> settings)
            {
                var previousAction = _viewModelInitializer;
                _viewModelInitializer = view =>
                {
                    previousAction?.Invoke(view);
                    if (view.DataContext is TViewModel viewModel)
                    {
                        settings?.Invoke(viewModel);
                    }
                };
                return this;
            }

            public IDialogBuilder<TView> Window(Action<Window> settings)
            {
                var previousAction = _windowSettings;
                _windowSettings = window =>
                {
                    previousAction?.Invoke(window);
                    settings?.Invoke(window);
                };
                return this;
            }

            public IDialogBuilder<TView> AsSingleton(object singletonKey = null)
            {
                _singletonArgs = (true, singletonKey);
                return this;
            }

            public T Build<T>(Func<T> windowProvider = null) where T : Window, new()
            {
                windowProvider ??= () => new T();

                var result = _singletonArgs.IsSingleton && SingletonWindows.ContainsKey(GetSingletonKey())
                    ? (T)SingletonWindows[GetSingletonKey()]
                    // ReSharper disable once PossibleNullReferenceException
                    : Application.Current.Dispatcher.Invoke(() => NewDialogWindow(windowProvider));

                return result;
            }

            private T NewDialogWindow<T>(Func<T> windowProvider) where T : Window, new()
            {
                var window = windowProvider();
                var content = _viewBuilder();
                window.Content = content;

                if (_singletonArgs.IsSingleton)
                {
                    SingletonWindows.TryAdd(GetSingletonKey(), window);
                    window.Closed += (sender, e) => SingletonWindows.TryRemove(GetSingletonKey(), out _);
                }

                _windowSettings?.Invoke(window);

                if (content.DataContext is DialogViewModel dialogViewModel)
                {
                    dialogViewModel.Window = window;
                }

                _viewModelInitializer?.Invoke(content);

                return window;
            }

            private object GetSingletonKey() => DialogBuilder.GetSingletonKey(typeof(TView), _singletonArgs.Key);
        }
    }
}
