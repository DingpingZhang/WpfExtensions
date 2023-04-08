using System.ComponentModel;

namespace Tests.Binding;

internal interface ITestObject
{
    TestObject? Child { get; set; }
}
