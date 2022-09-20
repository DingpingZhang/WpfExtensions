using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace WpfExtensions.Infrastructure.Commands;

public class RelayCommandAsync : RelayCommandBase, INotifyPropertyChanged
{
    private readonly Func<Task> _execute;
    private readonly Func<bool> _canExecute;
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


    public RelayCommandAsync(Func<Task> execute, Func<bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }


    protected override bool CanExecute(object parameter) => !IsExecuting && CanExecute();

    protected override async void Execute(object parameter) => await ExecuteAsync();

    public bool CanExecute() => _canExecute?.Invoke() ?? true;

    public async Task ExecuteAsync()
    {
        IsExecuting = true;
        var invoke = _execute?.Invoke();
        if (invoke != null) await invoke;
        IsExecuting = false;
    }
}
