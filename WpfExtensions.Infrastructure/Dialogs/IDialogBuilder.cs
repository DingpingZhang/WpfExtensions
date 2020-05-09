using System;
using System.Windows;

namespace WpfExtensions.Infrastructure.Dialogs
{
    public interface IDialogBuilder
    {
        IDialogBuilder View<TView>(Action<TView> settings) where TView : FrameworkElement;

        IDialogBuilder ViewModel<TViewModel>(Action<TViewModel> settings);

        IDialogBuilder Window<TWindow>(Action<TWindow> settings) where TWindow : Window;

        IDialogBuilder Configure<TView, TViewModel, TWindow>(Action<TView, TViewModel, TWindow> configure)
            where TView : FrameworkElement
            where TWindow : Window;

        IDialogBuilder AsSingleton(object singletonKey = null);

        T Build<T>(Func<T> windowProvider, bool updateSingletonConfigure = false) where T : Window;
    }
}
