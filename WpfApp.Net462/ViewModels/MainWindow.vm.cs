using Prism.Mvvm;
using System.Globalization;
using System.Windows.Input;
using WpfExtensions.Infrastructure.Commands;
using WpfExtensions.Xaml;

namespace WpfApp.Net462.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private static readonly CultureInfo En = new CultureInfo("en");
        private static readonly CultureInfo ZhCn = new CultureInfo("zh-CN");

        private bool _isLoading;
        private string _inputText;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        public ICommand LoadCommand { get; }

        public ICommand SwitchCultureCommand { get; }

        public ICommand NavigateCommand { get; }

        public MainWindowViewModel()
        {
            I18nManager.Instance.CurrentUICulture = ZhCn;

            LoadCommand = new RelayCommand(() => IsLoading = !IsLoading);
            SwitchCultureCommand = new RelayCommand(() =>
            {
                I18nManager.Instance.CurrentUICulture = I18nManager.Instance.CurrentUICulture.Equals(En) ? ZhCn : En;
            });
        }
    }
}
