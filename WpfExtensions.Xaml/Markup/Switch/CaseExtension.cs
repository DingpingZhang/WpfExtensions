using System;
using System.Windows.Markup;

namespace WpfExtensions.Xaml.Markup
{
    [MarkupExtensionReturnType(typeof(CaseExtension))]
    public class CaseExtension : MarkupExtension, ICase
    {
        internal int Index { get; set; } = SwitchExtension.InvalidIndex;

        [ConstructorArgument(nameof(Option))]
        public object Option { get; set; }

        [ConstructorArgument(nameof(Value))]
        public object Value { get; set; }

        public CaseExtension(object option, object value)
        {
            Option = option;
            Value = value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
