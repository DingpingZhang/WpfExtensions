# System.Windows.Extensions

## 1. `ComposeExtension`

Combine multiple Converters into one pipeline.

```xml
<Button Visibility="{Binding SampleCollection, Converter={markup:Compose
                    {StaticResource AnyConverter},
                    {StaticResource NotConverter},
                    {StaticResource BooleanToVisibilityConverter}}}" />
```

## 2. `IfExtension`

Using the `Conditional expression` in XAML.

```xml
<Button Command="{markup:If
                 {Binding BoolProperty},
                 {Binding OkCommand},
                 {Binding CancelCommand}}" />
```

```xml
<UserControl>
    <markup:If Condition="{Binding IsLoading}">
        <markup:If.True>
            <LoadingView />
        </markup:If.True>
        
        <markup:If.False>
        	<LoadedView />
        </markup:If.False>
    </markup:If>
</UserControl>
```

## 3. `I18nExtension`

Dynamically switch languages without restarting the app.

```xml
<TextBlock Text="{markup:I18n {x:Static language:UiStrings.MainWindow_Title}}" />
```

## 4. `StylesExtension` (In Progress)

```xml
<Button Style="{markup:Styles 
               {StaticResource FlatButtonStyle}, 
               {StaticResource AnimationStyle},
               ...}" />
```

## 5. `SwitchExtension` (In Progress)

```xml
<Image Source="{markup:Switch {Binding FileType},
               {Case {x:Static res:FileType.Music}, {StaticResource MusicIcon}},
               {Case {x:Static res:FileType.Video}, {StaticResource VideoIcon}},
               {Case {x:Static res:FileType.Picture}, {StaticResource PictureIcon}},
               ...
               {Default {StaticResource UnknownFileIcon}}}" />
```

