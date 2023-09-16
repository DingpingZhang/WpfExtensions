# WpfExtensions

English | [中文](./README.zh-CN.md)

This project comes from some scattered works I did while working on Wpf development, and is a supplement to existing Mvvm frameworks. They don't solve any major problems, they just provide some syntactic sugar and let people write a few lines of code less. Its services are not limited to Wpf development, other similar Xaml frameworks, such as Uwp, Maui, etc. should also be able to use, but I has never tested on other frameworks.

The project is structured as follows:

- `WpfExtensions.Xaml`：A number of `MarkupExtension`s are provided to simplify Xaml development.
- `WpfExtensions.Binding`：To simplify the code for property dependency updates, a function similar to the one in Vue.js for computed-property is provided.
- `WpfExtensions.Infrastructure`：Some scattered features, when the time is ripe, will be separated out and released as separate modules.

## NuGet

| Package                 | NuGet                                                                                                                   |
| ----------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| `WpfExtensions.Xaml`    | [![version](https://img.shields.io/nuget/v/WpfExtensions.Xaml.svg)](https://www.nuget.org/packages/WpfExtensions.Xaml) |
| `WpfExtensions.Binding` | [![version](https://img.shields.io/nuget/v/WpfExtensions.Binding.svg)](https://www.nuget.org/packages/WpfExtensions.Binding)   |

## 1. WpfExtensions.Binding

Brings some of the functionality of the Reactivity module from [Vue3](https://vuejs.org/api/) into Wpf.

> The term "observable" as used in the following documentation refers to objects that implement `INotifyPropertyChanged` or `INotifyCollectionChanged`.

### 1.1 `Watch`

Subscribe to an observable expression and trigger a callback function when its value changes.

```csharp
// See the source code for more overloads, whose signatures are consistent with vue3's `watch()`, and the vue3 documentation for examples.
Reactivity.Default.Watch(() => Width * Height, area => Debug.WriteLine(area));
```

### 1.2 `WatchDeep`

Deep traversal subscribes to an observable object and triggers a callback function when its properties, or the properties of its properties, change.

```csharp
// `path` will print out the path to the specific property that was changed.
Reactivity.Default.WatchDeep(obj, path => Debug.WriteLine(path))
```

### 1.3 `Computed`

Computed property that is an instance method of the `BindableBase` base class.

```csharp
public class ViewModel : BindableBase {
    // Can be bound to xaml to automatically notify Area changes when Width or Height changes.
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
