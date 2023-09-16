# WpfExtensions

[English](./README.md) | 中文

本项目源于作者在从事 Wpf 开发时的一些游戏之作，是对现有 Mvvm 框架的补充。要说解决了什么重大问题，到也没有，仅仅是提供了一些语法糖，让人少写几行代码而已。其服务对象也不局限于 Wpf 开发，其它类似的 Xaml 框架，如 Uwp、Maui 等应该也可以使用，只是作者从未在其它框架上测试过。

项目的结构如下：

- `WpfExtensions.Xaml`：提供了一些 `MarkupExtension` 以简化 Xaml 开发。
- `WpfExtensions.Binding`：为简化属性依赖更新的代码，提供了类似 Vue.js 中的计算属性功能。
- `WpfExtensions.Infrastructure`：一些杂项，等待时机成熟后将被分离出来，作为独立的模块发布。

## NuGet

| Package                 | NuGet                                                                                                                        |
| ----------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| `WpfExtensions.Xaml`    | [![version](https://img.shields.io/nuget/v/WpfExtensions.Xaml.svg)](https://www.nuget.org/packages/WpfExtensions.Xaml)       |
| `WpfExtensions.Binding` | [![version](https://img.shields.io/nuget/v/WpfExtensions.Binding.svg)](https://www.nuget.org/packages/WpfExtensions.Binding) |

## 1. WpfExtensions.Binding

将 [Vue3](https://vuejs.org/api/) 响应式模块的部分功能引入到了 Wpf 中。

> 以下文档中出现的“可观测”一词，指的是实现了 `INotifyPropertyChanged` 或 `INotifyCollectionChanged` 的对象。

### 1.1 `Watch`

订阅一个可观测的表达式，并在其值发生改变的时候，触发回调函数。

```csharp
// 更多重载见源码，其签名与 vue3 的 watch() 保持一致，使用示例亦可直接参考 vue3 文档。
Reactivity.Default.Watch(() => Width * Height, area => Debug.WriteLine(area));
```

### 1.2 `WatchDeep`

深度遍历地订阅一个可观测的对象，并在其属性、或属性的属性发生变化时，触发回调函数。

```csharp
// `path` 将打印出具体被修改的属性的路径。
Reactivity.Default.WatchDeep(obj, path => Debug.WriteLine(path))
```

### 1.3 `Computed`

计算属性，是 `BindableBase` 基类的实例方法。

```csharp
public class ViewModel : BindableBase {
    // 可 binding 到 xaml，当 Width 或 Height 发生改变时，自动通知 Area 的改变。
    public double Area => Computed(() => Width * Height);
}
```

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
