using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

// ReSharper disable once CheckNamespace
namespace WpfExtensions.Infrastructure.Extensions
{
    public static class WindowExtensions
    {
        #region Win32 API functions

        //private const int SW_SHOW_NORMAL = 1;
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        //[DllImport("user32.dll")]
        //private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hWnd, bool bInvert);
        #endregion

        public static IntPtr GetHandle(this Window @this) => new WindowInteropHelper(@this).Handle;

        public static bool ActivateWindow(this Window @this)
        {
            if (@this == null ||
                !(PresentationSource.FromVisual(@this) is HwndSource hwndSource)) return false;

            if (@this.Visibility != Visibility.Visible)
                @this.Visibility = Visibility.Visible;

            hwndSource.Handle.ActivateWindow();
            return true;
        }

        public static void ActivateWindow(this IntPtr @this)
        {
            if (IsIconic(@this))
            {
                ShowWindowAsync(@this, SW_RESTORE);
            }
            //ShowWindowAsync(@this, IsIconic(@this) ? SW_RESTORE : SW_SHOW_NORMAL);
            SetForegroundWindow(@this);
            FlashWindow(@this, true);
        }

        public static void ShowAndActivate(this Window window)
        {
            try
            {

                if (window.Dispatcher.CheckAccess())
                {
                    Action();
                }
                else
                {
                    window.Dispatcher.Invoke(Action);
                }
            }
            catch
            {
                Debug.Fail($"An exception occured in the {window.Content.GetType()} dialog.");
            }

            void Action()
            {
                window.Show();
                window.ActivateWindow();
            }
        }
    }
}
