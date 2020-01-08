using System.Collections.ObjectModel;
using System.Windows.Data;

namespace WpfExtensions.Xaml.Markup
{
    public class CaseCollection : Collection<ICase>
    {
        private readonly SwitchExtension _switchExtension;

        public CaseCollection(SwitchExtension switchExtension) => _switchExtension = switchExtension;

        protected override void InsertItem(int index, ICase item)
        {
            if (item.Value is BindingBase binding)
            {
                _switchExtension.Bindings.Add(binding);
                SetIndex(item, _switchExtension.Bindings.Count - 1);
            }
            else
            {
                base.InsertItem(index, item);
            }
        }

        private static void SetIndex(ICase item, int index)
        {
            switch (item)
            {
                case Case @case:
                    @case.Index = index;
                    break;
                case CaseExtension caseExtension:
                    caseExtension.Index = index;
                    break;
            }
        }
    }
}
