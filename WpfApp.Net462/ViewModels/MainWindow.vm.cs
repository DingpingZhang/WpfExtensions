using Prism.Mvvm;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfExtensions.Xaml;

namespace WpfApp.Net462.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private static readonly CultureInfo En = new("en");
        private static readonly CultureInfo ZhCn = new("zh-CN");

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

        public ICommand NavigateCommand { get; }

        public MainWindowViewModel()
        {
            I18nManager.Instance.CurrentUICulture = ZhCn;
        }

        public async Task LoadAsync(bool isLoading)
        {
            await Task.Delay(5000);

            IsLoading = !isLoading;
        }


        public void SwitchCulture()
        {
            I18nManager.Instance.CurrentUICulture = I18nManager.Instance.CurrentUICulture.Equals(En) ? ZhCn : En;
        }
    }
}
