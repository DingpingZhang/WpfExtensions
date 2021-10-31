using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace WpfExtensions.Binding
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        private readonly ComputedProperty _computedProperty;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected BindableBase()
        {
            _computedProperty = new ComputedProperty(RaisePropertyChanged);
        }

        protected IPropertyObserver Make(string propertyName) => _computedProperty.Make(propertyName);

        protected T Computed<T>(Expression<Func<T>> expression, T fallback, [CallerMemberName] string? propertyName = null)
        {
            return _computedProperty.Computed(expression, fallback, propertyName);
        }

        protected T? Computed<T>(Expression<Func<T>> expression, [CallerMemberName] string? propertyName = null)
        {
            return _computedProperty.Computed(expression, propertyName);
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
}
