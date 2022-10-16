using WpfExtensions.Binding;

namespace Tests.Binding;

internal class TestObject : BindableBase, ITestObject
{
    public static TestObject Default { get; } = new();

    private string _name = string.Empty;
    private int _number;
    private TestObject? _child;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    public int Double => Computed(() => Number * 2);

    public int Nested4x => Computed(() => Double * 2);

    TestObject? ITestObject.Child
    {
        get => _child;
        set => SetProperty(ref _child, value);
    }

    public static string GetName(string text) => text;
}
