using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace WpfExtensions.Xaml.Markup
{
    [ContentProperty(nameof(Cases))]
    public partial class SwitchExtension : MultiBindingExtensionBase
    {
        private Binding? _to;
        private int _toIndex = Constants.InvalidIndex;

        [ConstructorArgument(nameof(To))]
        public Binding? To
        {
            get => _to;
            set
            {
                if (_toIndex != Constants.InvalidIndex)
                    throw new InvalidOperationException();

                Bindings.Add(_to = value);
                _toIndex = Bindings.Count - 1;
            }
        }

        public CaseCollection Cases { get; }

        public SwitchExtension()
        {
            Cases = new CaseCollection(this);
            Converter = new MultiValueConverter(this);
        }

        private class MultiValueConverter : IMultiValueConverter
        {
            private readonly SwitchExtension _switchExtension;

            public MultiValueConverter(SwitchExtension switchExtension) => _switchExtension = switchExtension;

            public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                var currentOption = values[_switchExtension._toIndex];
                if (currentOption == DependencyProperty.UnsetValue) return Binding.DoNothing;

                var @case = _switchExtension.Cases.FirstOrDefault(item => Equals(currentOption, item.Label)) ??
                            _switchExtension.Cases.FirstOrDefault(item => Equals(Constants.DefaultLabel, item.Label));

                if (@case == null) return null;

                var index = @case.Index;
                return index == Constants.InvalidIndex ? @case.Value : values[index];
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }
}
