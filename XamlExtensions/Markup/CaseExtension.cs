using System;
using System.Windows.Markup;

namespace XamlExtensions.Markup
{
    [MarkupExtensionReturnType(typeof(CaseExtension))]
    public class CaseExtension : MarkupExtension
    {
        public const int InvalidIndex = -1;

        [ConstructorArgument(nameof(Option))]
        public object Option { get; set; }

        [ConstructorArgument(nameof(Value))]
        public object Value { get; set; }

        internal int Index { get; set; } = InvalidIndex;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
