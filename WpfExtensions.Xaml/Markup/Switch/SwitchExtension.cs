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
        internal const int InvalidIndex = -1;

        private Binding _condition;
        private int _conditionIndex = InvalidIndex;

        [ConstructorArgument(nameof(Condition))]
        public Binding Condition
        {
            get => _condition;
            set
            {
                if (_conditionIndex != InvalidIndex)
                    throw new InvalidOperationException();

                Bindings.Add(_condition = value);
                _conditionIndex = Bindings.Count - 1;
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

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                var currentOption = values[_switchExtension._conditionIndex];
                if (currentOption == DependencyProperty.UnsetValue) return Binding.DoNothing;

                var @case = _switchExtension.Cases.FirstOrDefault(item => Equals(currentOption, item.Label)) ??
                            _switchExtension.Cases.FirstOrDefault(item => Equals(CaseExtension.DefaultLabel, item.Label));

                if (@case == null) return null;

                var index = @case.Index;
                return index == InvalidIndex ? @case.Value : values[index];
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }
}
