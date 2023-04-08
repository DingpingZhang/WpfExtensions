using WpfExtensions.Binding;

namespace Tests.Binding;

internal class TestProperty : BindableBase, ITestObject
{
    private int _number;
    private TestObject? _child;

    public int Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    public int Double => Computed(() => Number * 2);

    public TestObject? Child
    {
        get => _child;
        set => SetProperty(ref _child, value);
    }
}
