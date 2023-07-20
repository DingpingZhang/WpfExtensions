using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using WpfExtensions.Binding;

namespace Tests.Binding;

public class Tests
{
    [Fact(DisplayName = "it should support static properties")]
    public void SupportStaticProperties()
    {
        var testObject = new TestObject();

        int count = 0;
        bool flag = false;
        var token = Reactivity.Default.Watch(() => testObject.Name == "expected", value =>
        {
            count++;
            flag = value;
        });

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
        var token = Reactivity.Default.Watch(
            () => TestObject.Default.Number % 2 == 0
                ? TestObject.Default.Child!.Name
                : TestObject.Default.Name,
            value =>
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
        TestObject testObject = TestObject.Default;

        int count = 0;
        int number = 0;
        var token = Reactivity.Default.Watch(() => testObject.Field.Child!.Number, value =>
        {
            count++;
            number += value;
        });

        testObject.Field.Child = new TestObject();
        Assert.Equal(1, count);

        testObject.Field.Child.Number++;
        Assert.Equal(1, number);
        Assert.Equal(2, count);

        testObject.Field.Child.Number = 10086;
        Assert.Equal(10087, number);
        Assert.Equal(3, count);

        token.Dispose();

        testObject.Field.Child.Number = 10086;
        Assert.Equal(10087, number);
        Assert.Equal(3, count);
    }

    [Fact(DisplayName = "it should support the 'as' keyword")]
    public void SupportAsKeyword()
    {
        int count = 0;
        string name = string.Empty;
        var token = Reactivity.Default.Watch(() => (TestObject.Default.Abstraction as TestObject)!.Name, value =>
        {
            count++;
            name = value;
        });

        TestObject.Default.Abstraction = new TestObject();
        var testObject = (TestObject)TestObject.Default.Abstraction;

        count = 0;

        testObject.Name = "42";
        Assert.Equal("42", name);
        Assert.Equal(1, count);

        testObject.Name = "42";
        Assert.Equal("42", name);
        Assert.Equal(1, count);

        testObject.Name = "24";
        Assert.Equal("24", name);
        Assert.Equal(2, count);

        TestObject.Default.Abstraction = new TestObject { Name = "10086" };
        Assert.Equal("10086", name);
        Assert.Equal(3, count);

        token.Dispose();

        testObject = (TestObject)TestObject.Default.Abstraction;
        testObject.Name = "123";
        Assert.Equal("10086", name);
        Assert.Equal(3, count);
    }

    [Fact(DisplayName = "it should support the cast operator")]
    public void SupportCastOperator()
    {
        TestObject.Default.Abstraction = new TestObject();

        int count = 0;
        string name = string.Empty;
        var token = Reactivity.Default.Watch(() => ((TestObject)TestObject.Default.Abstraction).Name, value =>
        {
            count++;
            name = value;
        });

        var testObject = (TestObject)TestObject.Default.Abstraction;

        testObject.Name = "42";
        Assert.Equal("42", name);
        Assert.Equal(1, count);

        testObject.Name = "42";
        Assert.Equal("42", name);
        Assert.Equal(1, count);

        testObject.Name = "24";
        Assert.Equal("24", name);
        Assert.Equal(2, count);

        TestObject.Default.Abstraction = new TestObject { Name = "10086" };
        Assert.Equal("10086", name);
        Assert.Equal(3, count);

        token.Dispose();

        testObject = (TestObject)TestObject.Default.Abstraction;
        testObject.Name = "123";
        Assert.Equal("10086", name);
        Assert.Equal(3, count);
    }

    [Fact(DisplayName = "it should cache old value")]
    public void ShouldCacheOldValue()
    {
        double previous = 0;
        double current = 0;
        int count = 0;

        TestObject.Default.Number = 2;
        Reactivity.Default.Watch(() => TestObject.Default.Number * TestObject.Default.Number / 2.0, (value, oldValue) =>
        {
            count++;
            current = value;
            previous = oldValue;
        });

        Assert.Equal(0, previous);
        Assert.Equal(0, current);
        Assert.Equal(0, count);

        TestObject.Default.Number = 3;
        Assert.Equal(2, previous);
        Assert.Equal(4.5, current);
        Assert.Equal(1, count);

        TestObject.Default.Number = 4;
        Assert.Equal(4.5, previous);
        Assert.Equal(8, current);
        Assert.Equal(2, count);
    }

    [Fact(DisplayName = "it should be able to cancel async calls")]
    public async Task ShouldCancelAsyncCall()
    {
        var testObject = new TestObject();

        string name = string.Empty;
        int count = 0;
        int actualCount = 0;
        Reactivity.Default.Watch(() => TestObject.GetName(testObject.Name), async (value, _, onCleanup) =>
        {
            bool disposed = false;
            onCleanup(() => disposed = true);

            await Task.Delay(TimeSpan.FromSeconds(3 - count++));

            if (!disposed)
            {
                name = value;
                actualCount++;
            }
        });

        Assert.Equal(string.Empty, name);
        Assert.Equal(0, count);
        Assert.Equal(0, actualCount);

        testObject.Name = "first";
        Assert.Equal(string.Empty, name);
        Assert.Equal(1, count);
        Assert.Equal(0, actualCount);

        testObject.Name = "secound";
        Assert.Equal(string.Empty, name);
        Assert.Equal(2, count);
        Assert.Equal(0, actualCount);

        testObject.Name = "third";
        Assert.Equal(string.Empty, name);
        Assert.Equal(3, count);
        Assert.Equal(0, actualCount);

        await Task.Delay(TimeSpan.FromSeconds(4));

        Assert.Equal("third", name);
        Assert.Equal(3, count);
        Assert.Equal(1, actualCount);

        testObject.Name = "fourth";
        Assert.Equal("fourth", name);
        Assert.Equal(4, count);
        Assert.Equal(2, actualCount);

    }

    [Fact(DisplayName = "it should watch computed properties")]
    public void ShouldWatchComputed()
    {
        var obj = new TestObject();

        int dummy = 0;

        _ = obj.Double;
        Reactivity.Default.Watch(() => obj.Double, value => dummy = value);

        Assert.Equal(0, dummy);

        obj.Number = 42;
        Assert.Equal(84, dummy);

        obj.Number = 3;
        Assert.Equal(6, dummy);
    }

    [Fact(DisplayName = "it should watch nested computed properties")]
    public void ShouldWatchNestedComputed()
    {
        var obj = new TestObject();

        int dummy = 0;

        _ = obj.Nested4x;
        Reactivity.Default.Watch(() => obj.Nested4x, value => dummy = value);

        Assert.Equal(0, dummy);

        obj.Number = 2;
        Assert.Equal(8, dummy);

        obj.Number = 3;
        Assert.Equal(12, dummy);
    }

    [Fact(DisplayName = "scope: it should collect watchers")]
    public void ShouldCollectWatchers()
    {
        var scope = Reactivity.Default.Scope();
        var scope2 = Reactivity.Default.Scope();

        int dummy = 0;
        int dummy2 = 0;
        var obj = new TestObject();

        using (scope.Begin())
        {
            Reactivity.Default.Watch(() => obj.Number, value => dummy = value);
            scope2.Run(() => Reactivity.Default.Watch(() => obj.Number * 2, value => dummy2 = value));
        }

        Assert.Equal(0, dummy);
        Assert.Equal(0, dummy2);
        obj.Number = 42;
        Assert.Equal(42, dummy);
        Assert.Equal(84, dummy2);

        scope.Dispose();

        obj.Number = 10086;
        Assert.Equal(42, dummy);
        Assert.Equal(20172, dummy2);

        scope2.Dispose();

        obj.Number = 2;
        Assert.Equal(42, dummy);
        Assert.Equal(20172, dummy2);
    }

    [Fact(DisplayName = "scope: it should collect nested scope")]
    public void ShouldCollectNestedScope()
    {
        var scope = Reactivity.Default.Scope();

        int dummy = 0;
        int dummy2 = 0;
        var obj = new TestObject();

        Scope scope2;
        using (scope.Begin())
        {
            Reactivity.Default.Watch(() => obj.Number, value => dummy = value);

            scope2 = Reactivity.Default.Scope();
            scope2.Run(() => Reactivity.Default.Watch(() => obj.Number * 2, value => dummy2 = value));
        }

        Assert.Equal(0, dummy);
        Assert.Equal(0, dummy2);
        obj.Number = 42;
        Assert.Equal(42, dummy);
        Assert.Equal(84, dummy2);

        scope.Dispose();
        Assert.True(scope.IsDisposed);
        Assert.True(scope2.IsDisposed);

        obj.Number = 10086;
        Assert.Equal(42, dummy);
        Assert.Equal(84, dummy2);
    }

    [Fact(DisplayName = "scope: it should be able to dispose sub-scope individually")]
    public void ShouldDisposeSubscope()
    {
        var scope = Reactivity.Default.Scope();

        int dummy = 0;
        int dummy2 = 0;
        var obj = new TestObject();

        using (scope.Begin())
        {
            Reactivity.Default.Watch(() => obj.Number, value => dummy = value);

            var scope2 = Reactivity.Default.Scope();
            scope2.Run(() => Reactivity.Default.Watch(() => obj.Number * 2, value => dummy2 = value));

            Assert.Equal(0, dummy);
            Assert.Equal(0, dummy2);
            obj.Number = 42;
            Assert.Equal(42, dummy);
            Assert.Equal(84, dummy2);

            scope2.Dispose();
            Assert.False(scope.IsDisposed);
            Assert.True(scope2.IsDisposed);

            obj.Number = 2;
            Assert.Equal(2, dummy);
            Assert.Equal(84, dummy2);
        }

        obj.Number = 10086;
        Assert.Equal(10086, dummy);
        Assert.Equal(84, dummy2);

        scope.Dispose();
        Assert.True(scope.IsDisposed);

        obj.Number = 24;
        Assert.Equal(10086, dummy);
        Assert.Equal(84, dummy2);
    }

    [Fact(DisplayName = "it should support computed property in BindableBase")]
    public void SupportComputedProperty()
    {
        var obj = new TestObject();
        var prop = new TestProperty();
        obj.Objects.Add(prop);

        int count = 0;
        int flag = 0;
        // read to trigger subscription.
        _ = prop.Double;
        Reactivity.Default.Watch(() => obj.Objects[0].Double, value =>
        {
            count++;
            flag = value;
        });

        prop.Number = 1;
        Assert.Equal(1, count);
        Assert.Equal(2, flag);
    }

    [Fact(DisplayName = "deep: it should watch a INotifyPropertyChanged instance")]
    public void SupportWatchDeepINotifyPropertyChanged()
    {
        var obj = new TestObject();
        int count = 0;
        Reactivity.Default.WatchDeep(obj, path => count++);

        Assert.Equal(0, count);

        obj.Number = 42;
        Assert.Equal(1, count);

        count = 0;
        obj.Child = new TestObject();
        Assert.Equal(1, count);

        count = 0;
        obj.Strings.Add("foo");
        // Count, Item[], Add()
        Assert.Equal(3, count);

        count = 0;
        obj.Child.Name = "foo";
        Assert.Equal(1, count);

        count = 0;
        obj.Child.Number = 42;
        Assert.Equal(1, count);

        count = 0;
        obj.Child = new TestObject();
        Assert.Equal(1, count);

        count = 0;
        var prop = new TestProperty();
        obj.Child.Objects.Add(new TestProperty());
        obj.Child.Objects.Add(prop);
        obj.Child.Objects.Add(new TestProperty());
        Assert.Equal(9, count);

        count = 0;
        prop.Number = 42;
        Assert.Equal(1, count);

        count = 0;
        obj.Child.Objects.Remove(prop);
        Assert.Equal(3, count);

        count = 0;
        prop.Number = 24;
        Assert.Equal(0, count);

        count = 0;
        obj.Child.Objects[0].Child = new TestObject();
        Assert.Equal(1, count);

        count = 0;
        obj.Child.Objects[0].Child!.Name = "foo";
        Assert.Equal(1, count);
    }

    [Fact(DisplayName = "deep: it should watch a INotifyCollectionChanged instance")]
    public void SupportWatchDeepINotifyCollectionChanged()
    {
        var col = new ObservableCollection<string> { "1", "2", "3" };

        int count = 0;
        Reactivity.Default.WatchDeep(col, path => count++);

        count = 0;
        col.Add("42");
        Assert.Equal(3, count);

        count = 0;
        col[0] = "10086";
        // Item[], Replace()
        Assert.Equal(2, count);

        var item1 = new TestProperty();
        var col2 = new ObservableCollection<TestProperty> { item1, new TestProperty() };

        Reactivity.Default.WatchDeep(col2, path => count++);

        count = 0;
        item1.Child = new TestObject();
        Assert.Equal(1, count);

        count = 0;
        item1.Child.Name = "foo";
        Assert.Equal(1, count);

        count = 0;
        col2[1].Number = 42;
        Assert.Equal(1, count);

        count = 0;
        col2[0] = new TestProperty();
        Assert.Equal(2, count);

        count = 0;
        item1.Number = 123;
        Assert.Equal(0, count);

        count = 0;
        col2[0].Number = 24;
        Assert.Equal(1, count);
    }
}
