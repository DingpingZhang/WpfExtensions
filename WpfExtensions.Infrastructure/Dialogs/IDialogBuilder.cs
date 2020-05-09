using System;
using System.Windows;

namespace WpfExtensions.Infrastructure.Dialogs
{
    public interface IDialogBuilder<out TView> where TView : FrameworkElement
    {
        IDialogBuilder<TView> View(Action<TView> settings);

        IDialogBuilder<TView> ViewModel<TViewModel>(Action<TViewModel> settings);

        IDialogBuilder<TView> Window(Action<Window> settings);

        IDialogBuilder<TView> AsSingleton(object singletonKey = null);

        T Build<T>(Func<T> windowProvider = null) where T : Window, new();
    }

    public interface IDialogBuilder
    {
        IDialogBuilder View<TView>(Action<TView> settings) where TView : FrameworkElement;

        IDialogBuilder ViewModel<TViewModel>(Action<TViewModel> settings);

        IDialogBuilder Window<TWindow>(Action<TWindow> settings) where TWindow : Window;

        IDialogBuilder AsSingleton(object singletonKey = null);

        T Build<T>(Func<T> windowProvider = null) where T : Window, new();
    }
}
