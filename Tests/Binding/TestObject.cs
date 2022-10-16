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

    TestObject? ITestObject.Child
    {
        get => _child;
        set => SetProperty(ref _child, value);
    }

    public static string GetName(string text) => text;
}
