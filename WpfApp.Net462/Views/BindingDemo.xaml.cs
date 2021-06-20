using WpfApp.Net462.ViewModels;

namespace WpfApp.Net462.Views
{
    /// <summary>
    /// Interaction logic for BindingDemo.xaml
    /// </summary>
    public partial class BindingDemo
    {
        public BindingDemo()
        {
            DataContext = new BindingDemoViewModel();
            InitializeComponent();
        }
    }
}
