using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WpfExtensions.Infrastructure.Extensions;

namespace WpfExtensions.Infrastructure.Commands;

public class RelayCommandAsync<T> : RelayCommandBase, INotifyPropertyChanged
{
    private readonly Func<T, Task> _execute;
    private readonly Func<T, bool> _canExecute;
    private bool _isExecuting;

    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            if (_isExecuting == value) return;
            _isExecuting = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExecuting)));
        }
    }


    public RelayCommandAsync(Func<T, Task> execute, Func<T, bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }


    protected override bool CanExecute(object parameter) => !IsExecuting && CanExecute(parameter.CastTo<T>());

    protected override async void Execute(object parameter) => await ExecuteAsync(parameter.CastTo<T>());

    public bool CanExecute(T parameter) => _canExecute?.Invoke(parameter) ?? true;

    public async Task ExecuteAsync(T parameter)
    {
        IsExecuting = true;
        var invoke = _execute?.Invoke(parameter);
        if (invoke != null) await invoke;
        IsExecuting = false;
    }
}
