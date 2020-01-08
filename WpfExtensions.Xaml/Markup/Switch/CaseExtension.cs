using System;
using System.Windows.Markup;

namespace WpfExtensions.Xaml.Markup
{
    [MarkupExtensionReturnType(typeof(CaseExtension))]
    [ContentProperty(nameof(Value))]
    public class CaseExtension : MarkupExtension
    {
        internal static readonly object DefaultOption = new object();

        internal int Index { get; set; } = SwitchExtension.InvalidIndex;

        [ConstructorArgument(nameof(Option))]
        public object Option { get; set; } = DefaultOption;

        [ConstructorArgument(nameof(Value))]
        public object Value { get; set; }

        public CaseExtension() { }

        public CaseExtension(object value)
        {
            Value = value;
        }

        public CaseExtension(object option, object value)
        {
            Option = option;
            Value = value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
