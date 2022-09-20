namespace WpfExtensions.Infrastructure.Mvvm;

public interface ITabItemSelectionChangedAware
{
    void OnSelected();

    void OnUnselected();
}
