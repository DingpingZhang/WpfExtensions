using System.Windows.Markup;

namespace WpfExtensions.Xaml.Markup
{
    [ContentProperty(nameof(Value))]
    public class Case : ICase
    {
        internal int Index { get; set; } = SwitchExtension.InvalidIndex;

        public object Option { get; set; }

        public object Value { get; set; }
    }
}
