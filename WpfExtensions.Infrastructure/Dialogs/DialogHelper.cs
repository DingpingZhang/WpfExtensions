using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace WpfExtensions.Infrastructure.Dialogs
{
    public static class DialogHelper
    {
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

        private static readonly ILog Logger = LogManager.GetLogger(typeof(DialogHelper));

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
                    frameworkElement.DataContext is DialogViewModel dialogViewModel &&
                    dialogViewModel.Result != null)
                {
                    tempResult = (TResult)dialogViewModel.Result;
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

        public static void ShowModal(this Window window) => HeadlessWindow.Using(window.ShowModal);

        public static void ShowModal(this Window @this, Window owner)
        {
            if (@this.Owner == null && owner == null)
                throw new InvalidOperationException($"Cannot show a modal dialog ({@this.GetFriendlyWindowName()}) that doesn't have an owner window. ");

            if (owner != null) @this.Owner = owner;

            IntPtr parentHandle = @this.Owner.GetHandle();
            EnableWindow(parentHandle, false);
            new ShowAndWaitHelper(@this).ShowAndWait(); // block
        }

        public static bool ShowModal<TResult>(this Window @this, out TResult result)
        {
            var returnValue = false;
            TResult dialogResult = default;

            HeadlessWindow.Using(owner => returnValue = @this.ShowModal(out dialogResult, owner));

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
                frameworkElement.DataContext is DialogViewModel dialogViewModel &&
                dialogViewModel.Result != null)
            {
                result = (TResult)dialogViewModel.Result;
                return true;
            }

            result = default;
            return false;
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
