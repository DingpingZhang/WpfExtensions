using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace WpfExtensions.Xaml.Markup
{
    public class CaseCollection : Collection<CaseExtension>
    {
        private readonly SwitchExtension _switchExtension;

        public CaseCollection(SwitchExtension switchExtension) => _switchExtension = switchExtension;

        protected override void InsertItem(int index, CaseExtension item)
        {
            if (ReferenceEquals(item.Label, Constants.DefaultLabel) &&
                Items.Any(it => ReferenceEquals(it.Label, Constants.DefaultLabel)))
            {
                throw new InvalidOperationException(
                    "A Switch markup extension must not contain more than one default Case markup extension.");
            }

            if (item.Value is BindingBase binding)
            {
                _switchExtension.Bindings.Add(binding);
                item.Index = _switchExtension.Bindings.Count - 1;
            }
            else
            {
                base.InsertItem(index, item);
            }
        }
    }
}
