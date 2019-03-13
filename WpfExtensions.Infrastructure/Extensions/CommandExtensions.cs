// ReSharper disable once CheckNamespace

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfExtensions.Infrastructure.Extensions
{
    public static class CommandExtensions
    {
        private class ThrottleCommand : ICommand
        {
            private readonly ICommand _command;
            private readonly TimeSpan _dueTime;
            private DateTime _lastExecutedTime = DateTime.MinValue;
            private object _lastParameter;
            private bool _isWaiting;

            public ThrottleCommand(ICommand command, TimeSpan dueTime)
            {
                _command = command;
                _dueTime = dueTime;
            }

            public async void Execute(object parameter) => await InnerExecuteAsync(parameter);
            //{
            //    var nowTime = DateTime.Now;
            //    if (_dueTime < nowTime - _lastExecutedTime)
            //    {
            //        _lastExecutedTime = nowTime;
            //        _command.Execute(parameter);
            //    }
            //}

            public bool CanExecute(object parameter) => _command.CanExecute(parameter);

            private async Task InnerExecuteAsync(object parameter)
            {
                _lastParameter = parameter;

                if (_isWaiting) return;

                var interval = DateTime.Now - _lastExecutedTime;
                if (_dueTime > interval)
                {
                    _isWaiting = true;
                    await Task.Delay(_dueTime - interval);
                    _isWaiting = false;
                }

                _lastExecutedTime = DateTime.Now;
                _command.Execute(_lastParameter);
            }

            public event EventHandler CanExecuteChanged
            {
                add => _command.CanExecuteChanged += value;
                remove => _command.CanExecuteChanged -= value;
            }
        }

        public static ICommand Throttle(this ICommand @this, TimeSpan dueTime)
        {
            return new ThrottleCommand(@this, dueTime);
        }

        public static void Invoke<T>(this T @this, object parameter = null) where T : ICommand
        {
            if (@this?.CanExecute(parameter) ?? false) @this.Execute(parameter);
        }
    }
}
