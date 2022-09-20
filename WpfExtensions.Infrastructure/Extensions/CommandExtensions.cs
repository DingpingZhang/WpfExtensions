using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfExtensions.Infrastructure.Extensions;

public static class CommandExtensions
{
    private abstract class CommandBase : ICommand
    {
        protected readonly ICommand _command;
        protected readonly TimeSpan _interval;

        public event EventHandler CanExecuteChanged
        {
            add => _command.CanExecuteChanged += value;
            remove => _command.CanExecuteChanged -= value;
        }

        protected CommandBase(ICommand command, TimeSpan interval)
        {
            _command = command;
            _interval = interval;
        }

        public bool CanExecute(object parameter) => _command.CanExecute(parameter);

        public abstract void Execute(object parameter);
    }

    private class ThrottleCommand : CommandBase
    {
        private volatile bool _isWaiting;
        private volatile object _lastParameter;
        private DateTime _lastTriggeredTime = DateTime.MinValue;

        public ThrottleCommand(ICommand command, TimeSpan interval) : base(command, interval) { }

        public override async void Execute(object parameter) => await InnerExecuteAsync(parameter);

        // RFC: http://introtorx.com/Content/v1.0.10621.0/13_TimeShiftedSequences.html#Throttle
        private async Task InnerExecuteAsync(object parameter)
        {
            _lastParameter = parameter;
            _lastTriggeredTime = DateTime.UtcNow;

            if (_isWaiting) return;
            _isWaiting = true;

            var actualInterval = DateTime.UtcNow - _lastTriggeredTime;
            while (_interval > actualInterval)
            {
                await Task.Delay(_interval - actualInterval);
                actualInterval = DateTime.UtcNow - _lastTriggeredTime;
            }

            _command.Execute(_lastParameter);

            _isWaiting = false;
        }
    }

    public static ICommand Throttle(this ICommand @this, TimeSpan dueTime)
    {
        return new ThrottleCommand(@this, dueTime);
    }

    public static void Invoke<T>(this T @this, object parameter = null) where T : ICommand
    {
        if (@this?.CanExecute(parameter) ?? false)
        {
            @this.Execute(parameter);
        }
    }
}
