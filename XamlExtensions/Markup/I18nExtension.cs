using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using XamlExtensions.ExtensionMethods;

namespace XamlExtensions.Markup
{

    [ContentProperty(nameof(Args))]
    [MarkupExtensionReturnType(typeof(object))]
    // ReSharper disable once InconsistentNaming
    public partial class I18nExtension : MultiBinding
    {
        private int _keyIndex;
        private ComponentResourceKey _key;

        public ComponentResourceKey Key
        {
            get => _key;
            set
            {
                _keyIndex = Bindings.Count;
                _key = value;
            }
        }

        public ArgCollection Args { get; }

        [ConstructorArgument(nameof(ResourceConverter))]
        public IValueConverter ResourceConverter { get; set; }

        public new IMultiValueConverter Converter
        {
            get => base.Converter;
            private set => base.Converter = value;
        }

        private class MultiValueConverter : IMultiValueConverter
        {
            private readonly I18nExtension _owner;

            public MultiValueConverter(I18nExtension owner) => _owner = owner;

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                switch (values[_owner._keyIndex])
                {
                    case string @string:
                        return values.Length > 1
                            ? string.Format(@string, _owner.Args.Indexes.Select(item => item.InBindings
                                ? values[item.Index]
                                : _owner.Args[item.Index]).ToArray())
                            : values.Single();
                    case Bitmap bitmap:
                        return bitmap.ToBitmapSource();
                    case Icon icon:
                        return icon.ToImageSource();
                    default:
                        return values[_owner._keyIndex];
                }
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }

    public class ArgCollection : Collection<object>
    {
        private readonly I18nExtension _owner;

        internal List<(bool InBindings, int Index)> Indexes = new List<(bool InBindings, int Index)>();

        public ArgCollection(I18nExtension owner) => _owner = owner;

        protected override void InsertItem(int index, object item)
        {
            if (item is BindingBase binding)
            {
                Indexes.Add((true, _owner.Bindings.Count));
                _owner.Bindings.Add(binding);
            }
            else
            {
                Indexes.Add((false, Count));
                base.InsertItem(index, item);
            }
        }
    }

    //[MarkupExtensionReturnType(typeof(object))]
    //// ReSharper disable once InconsistentNaming
    //public class I18nExtension : MarkupExtension
    //{
    //    [ConstructorArgument(nameof(Key))]
    //    public ComponentResourceKey Key { get; set; }

    //    [ConstructorArgument(nameof(Converter))]
    //    public IValueConverter Converter { get; set; }

    //    public I18nExtension() { }

    //    public I18nExtension(ComponentResourceKey key) => Key = key;

    //    public override object ProvideValue(IServiceProvider serviceProvider)
    //    {
    //        if (Key == null)
    //            throw new NullReferenceException($"{nameof(Key)} cannot be null at the same time.");

    //        return ProvideValueFromKey(serviceProvider, Key);
    //    }

    //    private object ProvideValueFromKey(IServiceProvider serviceProvider, ComponentResourceKey key)
    //    {
    //        if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget provideValueTarget))
    //            throw new ArgumentException(
    //                $"The {nameof(serviceProvider)} must implement {nameof(IProvideValueTarget)} interface.");

    //        if (provideValueTarget.TargetObject.GetType().FullName == "System.Windows.SharedDp") return this;

    //        return new Binding(nameof(I18nSource.Value))
    //        {
    //            Source = new I18nSource(key, provideValueTarget.TargetObject),
    //            Mode = BindingMode.OneWay,
    //            Converter = Converter
    //        }.ProvideValue(serviceProvider);
    //    }
    //}
}
