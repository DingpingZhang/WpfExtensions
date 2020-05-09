using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Prism.Logging;
using WpfExtensions.Infrastructure.Extensions;

namespace WpfExtensions.Infrastructure.Dialogs
{
    public static class DialogExtensions
    {
        private static readonly ILoggerFacade Logger = DefaultLogger.Get(typeof(DialogExtensions));

        /// <summary>
        /// Enables or disables mouse and keyboard input to the specified window or control. 
        /// When input is disabled, the window does not receive input such as mouse clicks and key presses. 
        /// When input is enabled, the window receives all input.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="bEnable"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        public static bool ShowDialog<TResult>(this Window window, out TResult result)
        {
            try
            {
                window.Dispatcher.Run(() => window.ShowDialog());
            }
            catch (Exception e)
            {
                Logger.Error($"An exception occurred in the {window.Content.GetType()} dialog.", e);
            }

            var canGetResult = false;
            TResult tempResult = default;
            window.Dispatcher.Run(() =>
            {
                if (window.Content is FrameworkElement frameworkElement &&
                    frameworkElement.DataContext is IDialogReturnable dialogReturnable &&
                    dialogReturnable.Result != null)
                {
                    tempResult = (TResult)dialogReturnable.Result;
                    canGetResult = true;
                }
            });

            if (canGetResult)
            {
                result = tempResult;
                return true;
            }

            result = default;
            return false;
        }

        public static void ShowModal(this Window window) => UsingHeadlessWindow(window.ShowModal);

        public static void ShowModal(this Window @this, Window owner)
        {
            Guards.ThrowIfNull(@this);

            if (@this.Owner == null && owner == null)
                throw new InvalidOperationException($"Cannot show a modal dialog ({@this.Content?.GetType()}) that doesn't have an owner window.");

            if (owner != null) @this.Owner = owner;

            IntPtr parentHandle = @this.Owner.GetHandle();
            EnableWindow(parentHandle, false);
            new ShowAndWaitHelper(@this).ShowAndWait(); // block
        }

        public static bool ShowModal<TResult>(this Window @this, out TResult result)
        {
            var returnValue = false;
            TResult dialogResult = default;

            UsingHeadlessWindow(owner => returnValue = @this.ShowModal(out dialogResult, owner));

            result = dialogResult;
            return returnValue;
        }

        public static bool ShowModal<TResult>(this Window @this, out TResult result, Window owner)
        {
            try
            {
                @this.ShowModal(owner);
            }
            catch (Exception e)
            {
                Logger.Error($"An exception occurred in the {@this.Content.GetType()} dialog.", e);
            }

            if (@this.Content is FrameworkElement frameworkElement &&
                frameworkElement.DataContext is IDialogReturnable dialogReturnable &&
                dialogReturnable.Result != null)
            {
                result = (TResult)dialogReturnable.Result;
                return true;
            }

            result = default;
            return false;
        }

        private static void Run(this Dispatcher dispatcher, Action action)
        {
            if (dispatcher.CheckAccess())
            {
                action?.Invoke();
            }
            else
            {
                dispatcher.Invoke(action);
            }
        }

        private static void UsingHeadlessWindow(Action<Window> callback)
        {
            var window = new Window
            {
                Title = "HeadlessWindow",
                Width = 0,
                Height = 0,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                ShowInTaskbar = false
            };

            // https://stackoverflow.com/a/4826741/5986595
            new WindowInteropHelper(window).EnsureHandle();
            callback?.Invoke(window);
            window.Close();
        }

        private sealed class ShowAndWaitHelper
        {
            private readonly Window _window;
            private DispatcherFrame _dispatcherFrame;

            internal ShowAndWaitHelper(Window window)
            {
                _window = window ?? throw new ArgumentNullException(nameof(window));
            }

            internal void ShowAndWait()
            {
                if (_dispatcherFrame != null)
                    throw new InvalidOperationException("Cannot call ShowAndWait while waiting for a previous call to ShowAndWait to return.");

                _window.Closed += OnWindowClosed;
                _window.ShowAndActivate();
                _dispatcherFrame = new DispatcherFrame();
                Dispatcher.PushFrame(_dispatcherFrame);
            }

            private void OnWindowClosed(object source, EventArgs eventArgs)
            {
                IntPtr winHandle = _window.GetHandle();
                EnableWindow(winHandle, false);

                if (_window.Owner != null)
                {
                    IntPtr parentHandle = _window.Owner.GetHandle();
                    // re-enable parent window
                    EnableWindow(parentHandle, true);
                    _window.Owner.ActivateWindow();
                }

                if (_dispatcherFrame == null) return;

                _window.Closed -= OnWindowClosed;
                _dispatcherFrame.Continue = false;
                _dispatcherFrame = null;
            }
        }
    }
}
