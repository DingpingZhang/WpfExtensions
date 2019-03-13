using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Prism.Commands;

namespace WpfExtensions.Infrastructure.Commands
{
    public class DelegateCommandAsync : DelegateCommandBase, INotifyPropertyChanged
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;

        public DelegateCommandAsync(Func<Task> execute, Func<bool> canExecute = null)
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

        protected override async void Execute(object parameter) => await Execute();

        protected override bool CanExecute(object parameter) => CanExecute() && !IsExecuting;

        public bool CanExecute() => _canExecute?.Invoke() ?? true;

        public async Task Execute()
        {
            IsExecuting = true;
            var invoke = _execute?.Invoke();
            if (invoke != null) await invoke;
            IsExecuting = false;
        }

        public DelegateCommandAsync ObservesProperty<T>(Expression<Func<T>> propertyExpression)
        {
            ObservesPropertyInternal(propertyExpression);
            return this;
        }
    }
}
