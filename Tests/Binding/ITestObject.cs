using System.ComponentModel;

namespace Tests.Binding;

internal interface ITestObject : INotifyPropertyChanged
{
    TestObject? Child { get; set; }
}
