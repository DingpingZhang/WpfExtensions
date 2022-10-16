using Microsoft.VisualStudio.TestPlatform.Utilities;
using WpfExtensions.Binding;

namespace Tests.Binding;

public class Tests
{
    [Fact(DisplayName = "it should support static properties")]
    public void SupportStaticProperties()
    {
        int count = 0;
        bool flag = false;
        var token = Reactivity.Default.Watch(() => TestObject.Default.Name == "expected", value =>
        {
            count++;
            flag = value;
        });

        TestObject testObject = TestObject.Default;

        testObject.Name = "expected";
        Assert.True(flag);
        Assert.Equal(1, count);

        testObject.Name = "unexpected";
        Assert.False(flag);
        Assert.Equal(2, count);

        testObject.Name = "unexpected";
        Assert.False(flag);
        Assert.Equal(2, count);

        token.Dispose();

        testObject.Name = "expected";
        Assert.False(flag);
        Assert.Equal(2, count);
    }

    [Fact(DisplayName = "it should support closure variables")]
    public void SupportClosureVariables()
    {
        TestObject testObject = TestObject.Default;

        int count = 0;
        bool flag = false;
        var token = Reactivity.Default.Watch(() => testObject.Number % 2 == 0, value =>
        {
            count++;
            flag = value;
        });

        testObject.Number = 8;
        Assert.True(flag);
        Assert.Equal(1, count);

        testObject.Number++;
        Assert.False(flag);
        Assert.Equal(2, count);

        token.Dispose();

        testObject.Number = 2;
        Assert.False(flag);
        Assert.Equal(2, count);
    }

    [Fact(DisplayName = "it should support conditional branching")]
    public void SupportConditionalBranching()
    {

        int count = 0;
        string name = string.Empty;
        var token = Reactivity.Default.Watch(() => TestObject.Default.Number % 2 == 0 ? (TestObject.Default as ITestObject).Child!.Name : TestObject.Default.Name, value =>
        {
            count++;
            name = value;
        });

        ITestObject testObject = TestObject.Default;
        var test = TestObject.Default;

        test.Name = "42";
        Assert.Equal(string.Empty, name);
        Assert.Equal(0, count);

        testObject.Child = new TestObject { Name = "initial" };
        Assert.Equal("initial", name);
        Assert.Equal(1, count);

        test.Name = "24";
        Assert.Equal("initial", name);
        Assert.Equal(1, count);

        test.Number++;
        Assert.Equal("24", name);
        Assert.Equal(2, count);

        testObject.Child!.Name = "10086";
        Assert.Equal("24", name);
        Assert.Equal(2, count);

        test.Name = "42";
        Assert.Equal("42", name);
        Assert.Equal(3, count);

        test.Number++;
        Assert.Equal("10086", name);
        Assert.Equal(4, count);

        testObject.Child!.Name = "42";
        Assert.Equal("42", name);
        Assert.Equal(5, count);

        token.Dispose();

        test.Number++;
        testObject.Child!.Name = "123";
        test.Name = "321";
        Assert.Equal("42", name);
        Assert.Equal(5, count);
    }

    [Fact(DisplayName = "it should support null checking in the property chain")]
    public void SupportNullChacking()
    {
        ITestObject testObject = TestObject.Default;

        int count = 0;
        int number = 0;
        var token = Reactivity.Default.Watch(() => testObject.Child!.Number, value =>
        {
            count++;
            number += value;
        });

        testObject.Child = new TestObject();
        Assert.Equal(1, count);

        testObject.Child.Number++;
        Assert.Equal(1, number);
        Assert.Equal(2, count);

        testObject.Child.Number = 10086;
        Assert.Equal(10087, number);
        Assert.Equal(3, count);

        token.Dispose();

        testObject.Child.Number = 10086;
        Assert.Equal(10087, number);
        Assert.Equal(3, count);
    }

    [Fact(DisplayName = "it should support the 'as' keyword")]
    public void SupportAsKeyword()
    {
        ITestObject testObject = TestObject.Default;
        testObject.Child = new TestObject();

        int count = 0;
        string name = string.Empty;
        var token = Reactivity.Default.Watch(() => (TestObject.Default as ITestObject).Child!.Name, value =>
        {
            count++;
            name = value;
        });

        testObject.Child.Name = "42";
        Assert.Equal("42", name);
        Assert.Equal(1, count);

        testObject.Child.Name = "42";
        Assert.Equal("42", name);
        Assert.Equal(1, count);

        testObject.Child.Name = "24";
        Assert.Equal("24", name);
        Assert.Equal(2, count);

        testObject.Child = new TestObject { Name = "10086" };
        Assert.Equal("10086", name);
        Assert.Equal(3, count);

        token.Dispose();

        testObject.Child.Name = "123";
        Assert.Equal("10086", name);
        Assert.Equal(3, count);
    }

    [Fact(DisplayName = "it should support the cast operator")]
    public void SupportCastOperator()
    {
        ITestObject testObject = TestObject.Default;

        int count = 0;
        int number = 0;
        var token = Reactivity.Default.Watch(() => ((ITestObject)TestObject.Default).Child!.Number, value =>
        {
            count++;
            number += value;
        });

        testObject.Child = new TestObject();

        testObject.Child.Number = 42;
        Assert.Equal(42, number);
        Assert.Equal(2, count);

        testObject.Child.Number = 42;
        Assert.Equal(42, number);
        Assert.Equal(2, count);

        testObject.Child.Number = 24;
        Assert.Equal(66, number);
        Assert.Equal(3, count);

        token.Dispose();

        testObject.Child.Number = 123;
        Assert.Equal(66, number);
        Assert.Equal(3, count);
    }
}
