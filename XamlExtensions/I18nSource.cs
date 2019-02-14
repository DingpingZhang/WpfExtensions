using System.ComponentModel;
using System.Windows;

namespace XamlExtensions
{
    public class I18nSource : INotifyPropertyChanged
    {
        private readonly ComponentResourceKey _key;

        public event PropertyChangedEventHandler PropertyChanged;

        public I18nSource(ComponentResourceKey key, object owner = null)
        {
            _key = key;

            switch (owner)
            {
                case FrameworkElement frameworkElement:
                    frameworkElement.Loaded += OnLoaded;
                    frameworkElement.Unloaded += OnUnloaded;
                    break;
                case FrameworkContentElement frameworkContentElement:
                    frameworkContentElement.Loaded += OnLoaded;
                    frameworkContentElement.Unloaded += OnUnloaded;
                    break;
                default:
                    OnLoaded(null, null);
                    break;
            }
        }

        public object Value => I18nManager.Instance.Get(_key);

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            I18nManager.Instance.CurrentUICultureChanged += OnCurrentUICultureChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            I18nManager.Instance.CurrentUICultureChanged -= OnCurrentUICultureChanged;
        }

        private void OnCurrentUICultureChanged(object sender, CurrentUICultureChangedEventArgs currentUiCultureChangedEventArgs)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }

        public static implicit operator I18nSource(ComponentResourceKey resourceKey) => new I18nSource(resourceKey);
    }
}
