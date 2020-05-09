using System;
using System.Windows.Markup;

namespace WpfExtensions.Xaml.Markup
{
    [MarkupExtensionReturnType(typeof(CaseExtension))]
    [ContentProperty(nameof(Value))]
    public class CaseExtension : MarkupExtension
    {
        internal static readonly object DefaultLabel = new object();

        internal int Index { get; set; } = SwitchExtension.InvalidIndex;

        [ConstructorArgument(nameof(Label))]
        public object Label { get; set; } = DefaultLabel;

        [ConstructorArgument(nameof(Value))]
        public object Value { get; set; }

        public CaseExtension() { }

        public CaseExtension(object value)
        {
            Value = value;
        }

        public CaseExtension(object option, object value)
        {
            Label = option;
            Value = value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
