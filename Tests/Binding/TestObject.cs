using System.Collections.ObjectModel;
using WpfExtensions.Binding;

namespace Tests.Binding;

internal class TestObject : BindableBase, ITestObject
{
    public static TestObject Default { get; } = new();

    public readonly TestProperty Field = new();

    private string _name = string.Empty;
    private int _number;
    private TestObject? _child;
    private ITestObject? _abstraction;

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

    public TestObject? Child
    {
        get => _child;
        set => SetProperty(ref _child, value);
    }

    public ITestObject? Abstraction
    {
        get => _abstraction;
        set => SetProperty(ref _abstraction, value);
    }

    public ObservableCollection<string> Strings { get; } = new();

    public ObservableCollection<TestProperty> Objects { get; } = new();

    public static string GetName(string text) => text;
}
