using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace WpfExtensions.Xaml.Markup
{
    [MarkupExtensionReturnType(typeof(ICommand))]
    [Localizability(LocalizationCategory.NeverLocalize)]
    public class CommandExtension : MarkupExtension, ICommand
    {
        private readonly string _executeName;
        private readonly string? _canExecuteName;

        private Action<object>? _execute;
        private Func<object, bool>? _canExecute;
        private FrameworkElement? _frameworkElement;
        private bool _isExecuting;

        public CommandExtension(string executeName)
        {
            _executeName = executeName;
        }

        public CommandExtension(string executeName, string canExecuteName)
            : this(executeName)
        {
            _canExecuteName = canExecuteName;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget provideValueTarget)
            {
                throw new ArgumentException(
                    $"The {nameof(serviceProvider)} must implement {nameof(IProvideValueTarget)} interface.");
            }

            object? targetObject = provideValueTarget.TargetObject;
            if (targetObject?.GetType().FullName == "System.Windows.SharedDp") return this;

            if (targetObject is not FrameworkElement frameworkElement)
            {
                throw new ArgumentException("The bound element must be derived from the FrameworkElement type.");
            }

            _frameworkElement = frameworkElement;

            return this;
        }

        #region Implements ICommand

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                object? context = _frameworkElement?.DataContext;
                if (context == null) return false;

                var methodName = _canExecuteName;
                if (methodName == null) return !_isExecuting;

                var method = context.GetType().GetMethod(methodName);

                if (method == null)
                {
                    Type contextType = context.GetType();
                    throw new NullReferenceException($"Not found the method named \"{methodName}\" in {contextType} type.");
                }

                var parameters = method.GetParameters();
                if (parameters.Length > 1)
                {
                    throw new InvalidOperationException($"The method named \"{methodName}\" must only have 0 or 1 parameters.");
                }

                var hasParameter = parameters.Length > 0;

                if (method.ReturnType != typeof(bool))
                {
                    throw new InvalidOperationException($"The method named \"{methodName}\" must return bool type.");
                }

                _canExecute = (args) => (bool)method.Invoke(context, hasParameter ? new[] { args } : null)!;
            }

            return !_isExecuting && _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (_execute == null)
            {
                object? context = _frameworkElement?.DataContext;
                if (context == null) return;

                var methodName = _executeName;
                var method = context.GetType().GetMethod(methodName);

                if (method == null)
                {
                    Type contextType = context.GetType();
                    throw new NullReferenceException($"Not found the method named \"{methodName}\" in {contextType} type.");
                }

                var parameters = method.GetParameters();
                if (parameters.Length > 1)
                {
                    throw new InvalidOperationException($"The method named \"{methodName}\" must only have 0 or 1 parameters.");
                }

                var hasParameter = parameters.Length > 0;
                var isTask = typeof(Task).IsAssignableFrom(method.ReturnType);

                _execute = async (args) =>
                {
                    if (_isExecuting) return;

                    object? result = method.Invoke(context, hasParameter ? new[] { args } : null);
                    if (isTask)
                    {
                        _isExecuting = true;
                        RaiseCanExecuteChanged();
                        await (Task)result!;
                        _isExecuting = false;
                        RaiseCanExecuteChanged();
                    }
                };
            }

            _execute(parameter);
        }

        protected virtual void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion
    }
}
