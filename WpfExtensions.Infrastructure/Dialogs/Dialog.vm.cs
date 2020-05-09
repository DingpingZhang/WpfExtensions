using System.Threading.Tasks;

namespace WpfExtensions.Infrastructure.Dialogs
{
    public abstract class DialogViewModel : ViewModelBase
    {
        private MonitorAwareWindow _window;

        public object Result { get; protected set; }

        public MonitorAwareWindow Window
        {
            get => _window;
            set
            {
                if (Equals(_window, value)) return;

                _window = value;
                value.SetCloseMode(false, false, OnClosing);
            }
        }

        public DelegateCommandAsync ConfirmCommand { get; protected set; }

        public DelegateCommandAsync CloseCommand { get; protected set; }

        protected DialogViewModel(IUnityContainer container) : base(container)
        {
            ConfirmCommand = new DelegateCommandAsync(async () =>
            {
                if (await OnConfirmedAsync())
                {
                    CloseWindow();
                }
            }, () => !CloseCommand.IsExecuting).ObservesProperty(() => CloseCommand.IsExecuting);

            CloseCommand = new DelegateCommandAsync(async () =>
            {
                if (await OnClosingAsync())
                {
                    Result = null;
                    CloseWindow();
                }
            }, () => !ConfirmCommand.IsExecuting).ObservesProperty(() => ConfirmCommand.IsExecuting);
        }

        protected virtual Task<bool> OnConfirmedAsync() => Task.Run(() => OnConfirmed());

        protected virtual Task<bool> OnClosingAsync() => Task.Run(() => OnClosing());

        protected virtual bool OnConfirmed() => true;

        protected virtual bool OnClosing() => true;

        protected void CloseWindow()
        {
            InvokeOnUiThread(() => Window?.Close());
        }
    }
}
