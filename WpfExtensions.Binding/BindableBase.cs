using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace WpfExtensions.Binding;

/// <summary>
/// The common features of <see cref="INotifyPropertyChanged"/> are implemented.
/// </summary>
public abstract class BindableBase : INotifyPropertyChanged
{
    private readonly IDictionary<string, IStrongBox> _cache = new Dictionary<string, IStrongBox>();

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Computed property: Watch for changes to an expression and automatically notify UI updates when it changes.
    /// </summary>
    /// <typeparam name="T">The type of the expression.</typeparam>
    /// <param name="expression">The computed expression.</param>
    /// <param name="propertyName">The changed property name.</param>
    /// <returns>The computed result.</returns>
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

    /// <summary>
    /// An implementation of the property setter that automatically triggers the <see cref="INotifyPropertyChanged"/> event.
    /// </summary>
    /// <typeparam name="T">The type of this property.</typeparam>
    /// <param name="storage">The back field of this property.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The name of this property.</param>
    /// <returns>Returns a bool value indicating whether a change has occurred.</returns>
    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

        storage = value;
        RaisePropertyChanged(propertyName);

        return true;
    }

    /// <summary>
    /// An implementation of the property setter that automatically triggers the <see cref="INotifyPropertyChanged"/> event.
    /// </summary>
    /// <typeparam name="T">The type of this property.</typeparam>
    /// <param name="storage">The back field of this property.</param>
    /// <param name="value">The new value.</param>
    /// <param name="onChanged">A callback function that will be called when the property changes.</param>
    /// <param name="propertyName">The name of this property.</param>
    /// <returns>Returns a bool value indicating whether a change has occurred.</returns>
    protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

        storage = value;
        onChanged();
        RaisePropertyChanged(propertyName);

        return true;
    }

    /// <summary>
    /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The changed property name.</param>
    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    /// <param name="e">The arguments of this event.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }
}
