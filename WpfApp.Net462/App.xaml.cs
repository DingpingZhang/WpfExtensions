using System.Windows;
using WpfApp.Net462.I18nRes;
using WpfExtensions.Xaml;

namespace WpfApp.Net462
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            I18nManager.Instance.Add(Resource.ResourceManager);
        }
    }
}
