using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace WpfExtensions.Xaml.Markup
{
    public class CaseCollection : Collection<CaseExtension>
    {
        private readonly SwitchExtension _switchExtension;

        public CaseCollection(SwitchExtension switchExtension) => _switchExtension = switchExtension;

        protected override void InsertItem(int index, CaseExtension item)
        {
            if (item.Value is BindingBase binding)
            {
                _switchExtension.Bindings.Add(binding);
                item.Index = _switchExtension.Count++;
            }
            else
            {
                base.InsertItem(index, item);
            }
        }
    }

    public partial class SwitchExtension : MultiBinding
    {
        internal int Count = 0;

        private Binding _condition;
        private int _conditionIndex;

        [ConstructorArgument(nameof(Condition))]
        public Binding Condition
        {
            get => _condition;
            set
            {
                if (_conditionIndex != CaseExtension.InvalidIndex)
                    throw new InvalidOperationException();

                _condition = value;
                _conditionIndex = Count++;
            }
        }

        [ConstructorArgument(nameof(Cases))]
        public CaseCollection Cases { get; }

        [ConstructorArgument(nameof(Default))]
        public object Default { get; set; }

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
                var @case = _switchExtension.Cases.FirstOrDefault(item => Equals(currentOption, item.Option));

                return @case != null
                    ? @case.Index != CaseExtension.InvalidIndex
                        ? values[@case.Index]
                        : @case.Value
                    : null;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
