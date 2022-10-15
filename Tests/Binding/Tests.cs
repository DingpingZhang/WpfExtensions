using Microsoft.VisualStudio.TestPlatform.Utilities;
using WpfExtensions.Binding.Expressions;

namespace Tests.Binding;

public class Tests
{
    [Fact(DisplayName = "it should support static properties")]
    public void SupportStaticProperties()
    {

    }

    [Fact(DisplayName = "it should support closure variables")]
    public void SupportClosureVariables()
    {

    }

    [Fact(DisplayName = "it should support conditional branching")]
    public void SupportConditionalBranching()
    {

    }

    [Fact(DisplayName = "it should support null checking in the property chain")]
    public void SupportNullChacking()
    {
        ITestObject testObject = TestObject.Default;

        int count = 0;
        int number = 0;
        var token = ExpressionObserver.Observes(() => testObject.Child!.Number, (value, e) =>
        {
            count++;
            number += value;
            Assert.Null(e);
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
        var token = ExpressionObserver.Observes(() => (TestObject.Default as ITestObject).Child!.Name, (value, e) =>
        {
            Assert.Null(e);
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

        token.Dispose();

        testObject.Child.Name = "123";
        Assert.Equal("24", name);
        Assert.Equal(2, count);
    }

    [Fact(DisplayName = "it should support the cast operator")]
    public void SupportCastOperator()
    {
        ITestObject testObject = TestObject.Default;
        testObject.Child = new TestObject();

        int count = 0;
        int number = 0;
        var token = ExpressionObserver.Observes(() => ((ITestObject)TestObject.Default).Child!.Number, (value, e) =>
        {
            Assert.Null(e);
            count++;
            number += value;
        });

        testObject.Child.Number = 42;
        Assert.Equal(42, number);
        Assert.Equal(1, count);

        testObject.Child.Number = 42;
        Assert.Equal(42, number);
        Assert.Equal(1, count);

        testObject.Child.Number = 24;
        Assert.Equal(66, number);
        Assert.Equal(2, count);

        token.Dispose();

        testObject.Child.Number = 123;
        Assert.Equal(66, number);
        Assert.Equal(2, count);
    }
}
