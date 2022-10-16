using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace WpfExtensions.Binding;

public abstract class BindableBase : INotifyPropertyChanged
{
    private readonly IDictionary<string, IStrongBox> _cache = new Dictionary<string, IStrongBox>();

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    protected T Computed<T>(Expression<Func<T>> expression, [CallerMemberName] string propertyName = null!)
    {
        if (!_cache.ContainsKey(propertyName))
        {
            _cache.Add(propertyName, new StrongBox<T>(expression.Compile()()));
            Reactivity.Default.Watch(expression, value =>
            {
                var storage = (StrongBox<T>)_cache[propertyName];
                storage.Value = value;

                // Notify ui to pull the latest value after updating the storage.
                RaisePropertyChanged(propertyName);
            });
        }

        return ((StrongBox<T>)_cache[propertyName]).Value;
    }

    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

        storage = value;
        RaisePropertyChanged(propertyName);

        return true;
    }

    protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

        storage = value;
        onChanged();
        RaisePropertyChanged(propertyName);

        return true;
    }

    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }
}
