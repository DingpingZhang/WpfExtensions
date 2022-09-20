using System;

namespace WpfExtensions.Infrastructure.Mvvm;

public class ViewModelResolver : IViewModelResolver
{
    private readonly Func<IContainerProvider> _containerFactory;
    private Action<object, object, IContainerProvider> _configureViewAndViewModel;
    private IContainerProvider _container;

    public IContainerProvider Container => _container ??= _containerFactory();

    public ViewModelResolver(Func<IContainerProvider> containerFactory)
    {
        Guards.ThrowIfNull(containerFactory);

        _containerFactory = containerFactory;
    }

    public object ResolveViewModelForView(object view, Type viewModelType)
    {
        var viewModel = Container.Resolve(viewModelType);
        _configureViewAndViewModel?.Invoke(view, viewModel, Container);

        return viewModel;
    }

    public IViewModelResolver IfDerivedFrom<TView, TViewModel>(Action<TView, TViewModel, IContainerProvider> configuration)
    {
        var previousAction = _configureViewAndViewModel;
        _configureViewAndViewModel = (view, viewModel, container) =>
        {
            previousAction?.Invoke(view, viewModel, container);
            if (view is TView tView && viewModel is TViewModel tViewModel)
            {
                configuration?.Invoke(tView, tViewModel, container);
            }
        };
        return this;
    }

    public IViewModelResolver IfDerivedFrom<TView>(Type genericInterfaceType, Action<TView, object, IGenericInterface, IContainerProvider> configuration)
    {
        var previousAction = _configureViewAndViewModel;
        _configureViewAndViewModel = (view, viewModel, container) =>
        {
            previousAction?.Invoke(view, viewModel, container);
            var interfaceInstance = viewModel.AsGenericInterface(genericInterfaceType);
            if (view is TView tView && interfaceInstance != null)
            {
                configuration?.Invoke(tView, viewModel, interfaceInstance, container);
            }
        };
        return this;
    }
}
