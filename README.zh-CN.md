# WpfExtensions

[English](./README.md) | 中文

本项目源于作者在从事 Wpf 开发时的一些游戏之作，是对现有 Mvvm 框架的补充。要说解决了什么重大问题，到也没有，仅仅是提供了一些语法糖，让人少写几行代码而已。其服务对象也不局限于 Wpf 开发，其它类似的 Xaml 框架，如 Uwp、Maui 等应该也可以使用，只是作者从未在其它框架上测试过。

项目的结构如下：

- `WpfExtensions.Xaml`：提供了一些 `MarkupExtension` 以简化 Xaml 开发。
- `WpfExtensions.Binding`：为简化属性依赖更新的代码，提供了类似 Vue.js 中的计算属性功能。
- `WpfExtensions.Infrastructure`：一些杂项，等待时机成熟后将被分离出来，作为独立的模块发布。

## NuGet

| Package                 | NuGet                                                                                                                   |
| ----------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| `WpfExtensions.Xaml`    | [![version](https://img.shields.io/badge/version-1.2.0-brightgreen)](https://www.nuget.org/packages/WpfExtensions.Xaml) |
| `WpfExtensions.Binding` | [![version](https://img.shields.io/badge/version-0.1.1-orange)](https://www.nuget.org/packages/WpfExtensions.Binding)   |

## 1. WpfExtensions.Binding

该模块是为了解决属性更新依赖的问题，开发中经常会遇到：一个属性的值，是由其它多个数据计算得到的，那么当这些被依赖的数据发生改变时，结果属性也需要通知 UI 更新。如：`RectArea = Width * Height`，该公式中的三个属性都需要绑定到 UI 上，并且输入 `Width` 和 `Height` 后，`RectArea` 的显示将自动刷新。

为达到这一效果，传统地实现如下，不难发现：写起来挺麻烦的，每个被依赖的属性中，都要加一行代码去通知结果属性刷新。

```csharp
// View Model
public double Width {
    get => field;
    set {
        if (SetProperty(ref field, value)) {
            RaisePropertyChanged(nameof(RectArea));
        }
    }
}

public double Height {
    get => field;
    set {
        if (SetProperty(ref field, value)) {
            RaisePropertyChanged(nameof(RectArea));
        }
    }
}

public double RectArea => Width * Height;
```

那么使用 `WpfExtensions.Binding` 之后，是否可以短一点呢？

```csharp
// View Model is derived from WpfExtensions.Binding.BindableBase.
public double Width {
    get => field;
    set => SetProperty(ref field, value);
}

public double Height {
    get => field;
    set => SetProperty(ref field, value);
}

public double RectArea => Computed(() => Width * Height);
```

或许这个例子过于简单，但想象：如果同一个属性影响多个结果，那么就要在该属性中 Raise 多个结果属性。这样错综复杂的更新依赖关系，是不容易维护的。好吧，就算一般项目里不会出现比较复杂的更新依赖，那反正这个 Binding 库就十几 KB，引用了又不亏。

## 2. WpfExtensions.Xaml

## 0. **\*New** `CommandExtension`

- View (XAML):

```xml
<Element Command={markup:Command Execute} />
<Element Command={markup:Command ExecuteWithArgumentAsync, CanExecute}
         CommandParameter={Binding Argument} />
```

- View Model (\*.cs):

```csharp
class ViewModel
{
    public void Execute() {}

    public void ExecuteWithArgument(string arg) {}

    // The `Execute` method supports async, and its default `Can Execute` method will disable the command when it is busy.

    public Task ExecuteAsync() => Task.Completed;

    public Task ExecuteWithArgumentAsync(string arg) => Task.Completed;

    // The `Can Execute` method does not support async.

    public bool CanExecute() => true;

    public bool CanExecuteWithArgument(string arg) => true;
}
```

## 1. `ComposeExtension`

Combine multiple Converters into one pipeline.

```xml
<TextBlock Visibility="{Binding DescriptionText, Converter={markup:Compose
                       {StaticResource IsNullOrEmptyOperator},
                       {StaticResource NotConverter},
                       {StaticResource BooleanToVisibilityConverter}}}"
           Text="{Binding DescriptionText}" />
```

## 2. `IfExtension`

Using the `Conditional expression` in XAML.

```xml
<Button Command="{markup:If {Binding BoolProperty},
                            {Binding OkCommand},
                            {Binding CancelCommand}}" />
```

```xml
<UserControl>
    <markup:If Condition="{Binding IsLoading}">
        <markup:If.True>
            <views:LoadingView />
        </markup:If.True>
        <markup:If.False>
            <views:LoadedView />
        </markup:If.False>
    </markup:If>
</UserControl>
```

## 3. `SwitchExtension`

Using the `Switch expression` in XAML.

```xml
<Image Source="{markup:Switch {Binding FileType},
                              {Case {x:Static res:FileType.Music}, {StaticResource MusicIcon}},
                              {Case {x:Static res:FileType.Video}, {StaticResource VideoIcon}},
                              {Case {x:Static res:FileType.Picture}, {StaticResource PictureIcon}},
                              ...
                              {Case {StaticResource UnknownFileIcon}}}" />
```

```xml
<UserControl>
    <Switch To="{Binding SelectedViewName}">
        <Case Label="View1">
            <views:View1 />
        </Case>
        <Case Label="{x:Static res:Views.View2}">
            <views:View2 />
        </Case>
        <Case>
            <views:View404 />
        </Case>
    </Switch>
</UserControl>
```

## 4. `I18nExtension`

Dynamically switch the culture resource without restarting the app.

```xml
<TextBlock Text="{markup:I18n {x:Static languages:UiStrings.MainWindow_Title}}" />
<TextBlock Text="{markup:I18nString {x:Static languages:UiStrings.SayHello}, {Binding Username}}" />
<TextBlock Text="{markup:I18nString {x:Static languages:UiStrings.StringFormat},
                                    {Binding Arg0},
                                    {Binding Arg1},
                                    ...,
                                    {Binding Arg15}}" />
```

## 5. `StylesExtension` (In Progress)

```xml
<Button Style="{markup:Styles {StaticResource FlatButtonStyle},
                              {StaticResource AnimationStyle},
                              ...}" />
```
