using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfExtensions.Infrastructure.Extensions;

namespace WpfExtensions.Infrastructure.Mvvm;

public class TabControlHelper
{
    #region Switch Aware

    public static readonly DependencyProperty NotifySelectionChangedProperty = DependencyProperty.RegisterAttached(
        "NotifySelectionChanged", typeof(bool), typeof(TabControlHelper), new PropertyMetadata(default(bool), OnAwareSelectionChangedChanged));

    public static void SetNotifySelectionChanged(DependencyObject element, bool value)
    {
        element.SetValue(NotifySelectionChangedProperty, value);
    }

    public static bool GetNotifySelectionChanged(DependencyObject element)
    {
        return (bool)element.GetValue(NotifySelectionChangedProperty);
    }

    private static void OnAwareSelectionChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var frameworkElement = (FrameworkElement)d;

        if ((bool)e.NewValue)
            frameworkElement.Loaded += OnLoaded;
        else
            frameworkElement.Loaded -= OnLoaded;
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        var contentControl = (ContentControl)sender;
        var tab = contentControl.TryFindParent<TabControl>();

        if (tab == null) return;

        tab.SelectionChanged -= OnSelectionChanged;
        tab.SelectionChanged += OnSelectionChanged;
    }

    private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.OriginalSource != sender) return;

        var toTabItem = e.AddedItems.Cast<FrameworkElement>().SingleOrDefault();
        var fromTabItem = e.RemovedItems.Cast<FrameworkElement>().SingleOrDefault();

        if (toTabItem == null || fromTabItem == null) return;

        if (GetNotifySelectionChanged(fromTabItem))
            (fromTabItem.DataContext as ITabItemSelectionChangedAware)?.OnUnselected();

        if (GetNotifySelectionChanged(toTabItem))
            (toTabItem.DataContext as ITabItemSelectionChangedAware)?.OnSelected();
    }

    #endregion
}
