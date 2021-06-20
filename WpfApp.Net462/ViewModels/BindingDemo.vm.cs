using System;
using WpfExtensions.Binding;

namespace WpfApp.Net462.ViewModels
{
    public class BindingDemoViewModel : BindableBase
    {
        private double _a;
        private double _b;
        private double _c;

        public double A
        {
            get => _a;
            set => SetProperty(ref _a, value);
        }

        public double B
        {
            get => _b;
            set => SetProperty(ref _b, value);
        }

        public double C
        {
            get => _c;
            set => SetProperty(ref _c, value);
        }

        //public (double x1, double x2) Result => Computed(() => Calculate(A, B, C));
        public (double x1, double x2) Result => Calculate(A, B, C);

        private static (double x1, double x2) Calculate(double a, double b, double c)
        {
            double temp1 = b * b - 4 * a * c;

            return temp1 switch
            {
                0 => (-b / (2 * a), -b / (2 * a)),
                > 0 => (-b + Math.Sqrt(temp1) / (2 * a), -b - Math.Sqrt(temp1) / (2 * a)),
                _ => (double.NaN, double.NaN),
            };
        }

        public BindingDemoViewModel()
        {
            Make(nameof(Result)).Observes(() => EachOf(A, B, C));
        }

        private static string EachOf(params object[] _) => string.Empty;
    }
}
