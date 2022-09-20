using System;
using System.Linq;
using System.Windows;
using WpfExtensions.Infrastructure.Mvvm;

// ReSharper disable once CheckNamespace
namespace WpfExtensions.Infrastructure.Extensions;

public static class ViewModelResolverExtensions
{
    public static IViewModelResolver UseViewLoadedAndUnloadedAware(this IViewModelResolver @this) => @this
        .IfDerivedFrom<IViewLoadedAndUnloadedAware>((view, viewModel) =>
        {
            view.Loaded += (sender, e) => viewModel.OnLoaded();
            view.Unloaded += (sender, e) => viewModel.OnUnloaded();
        })
        .IfDerivedFrom(typeof(IViewLoadedAndUnloadedAware<>), (view, viewModel, interfaceInstance) =>
        {
            var viewType = view.GetType();
            var viewTypeFromInterface = interfaceInstance.GenericArguments.Single();
            if (viewTypeFromInterface != viewType)
            {
                throw new InvalidOperationException(
                    $"The type of the view is {viewType}, " +
                    $"but the IViewLoadedAndUnloadedAware<{viewTypeFromInterface.Name}> tried to " +
                    $"extract an instance of the {viewTypeFromInterface} type. ");
            }

            var onLoadedMethod = interfaceInstance.GetMethod<Action<object>>("OnLoaded", viewType);
            var onUnloadedMethod = interfaceInstance.GetMethod<Action<object>>("OnUnloaded", viewType);

            view.Loaded += (sender, args) => onLoadedMethod(sender);
            view.Unloaded += (sender, args) => onUnloadedMethod(sender);
        });

    public static IViewModelResolver UseTabItemSelectionChangedAware(this IViewModelResolver @this) => @this
        .IfDerivedFrom<ITabItemSelectionChangedAware>((view, viewModel) =>
        {
            TabControlHelper.SetNotifySelectionChanged(view, true);
        });

    public static IViewModelResolver IfDerivedFrom<TViewModel>(this IViewModelResolver @this, Action<FrameworkElement, TViewModel> configuration)
    {
        Guards.ThrowIfNull(@this, configuration);

        return @this.IfDerivedFrom<FrameworkElement, TViewModel>((view, viewModel, container) => configuration(view, viewModel));
    }

    public static IViewModelResolver IfDerivedFrom<TViewModel>(this IViewModelResolver @this, Action<FrameworkElement, TViewModel, IContainerProvider> configuration)
    {
        Guards.ThrowIfNull(@this, configuration);

        return @this.IfDerivedFrom(configuration);
    }

    public static IViewModelResolver IfDerivedFrom(this IViewModelResolver @this, Type genericInterfaceType, Action<FrameworkElement, object, IGenericInterface> configuration)
    {
        Guards.ThrowIfNull(@this, configuration);

        return @this.IfDerivedFrom<FrameworkElement>(
            genericInterfaceType,
            (view, viewModel, interfaceInstance, container) => configuration(view, viewModel, interfaceInstance));
    }
}
