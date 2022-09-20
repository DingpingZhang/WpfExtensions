using System;

namespace WpfExtensions.Infrastructure.Mvvm;

public interface IViewModelResolver
{
    object ResolveViewModelForView(object view, Type viewModelType);

    IViewModelResolver IfDerivedFrom<TView, TViewModel>(Action<TView, TViewModel, IContainerProvider> configuration);

    IViewModelResolver IfDerivedFrom<TView>(Type genericInterfaceType, Action<TView, object, IGenericInterface, IContainerProvider> configuration);
}
