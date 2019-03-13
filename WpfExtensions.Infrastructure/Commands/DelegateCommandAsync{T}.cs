using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Prism.Commands;

namespace WpfExtensions.Infrastructure.Commands
{
    public class DelegateCommandAsync<T> : DelegateCommandBase, INotifyPropertyChanged
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool> _canExecute;
        private bool _isExecuting;

        public DelegateCommandAsync(Func<T, Task> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                if (_isExecuting == value) return;
                _isExecuting = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExecuting)));
                OnCanExecuteChanged();
            }
        }

        protected override async void Execute(object parameter) => await Execute((T)parameter);

        protected override bool CanExecute(object parameter) => CanExecute((T)parameter) && !IsExecuting;

        public bool CanExecute(T parameter) => _canExecute?.Invoke(parameter) ?? true;

        public async Task Execute(T parameter)
        {
            IsExecuting = true;
            var invoke = _execute?.Invoke(parameter);
            if (invoke != null) await invoke;
            IsExecuting = false;
        }

        public DelegateCommandAsync<T> ObservesProperty<TType>(Expression<Func<TType>> propertyExpression)
        {
            ObservesPropertyInternal(propertyExpression);
            return this;
        }
    }
}
