# System.Windows.Extensions

## 1. `ComposeExtension`

Combine multiple Converters into one pipeline.

```xml
<Button Visibility="{Binding SampleCollection, Converter={markupEx:Compose
                    {StaticResource AnyConverter},
                    {StaticResource NotConverter},
                    {StaticResource BooleanToVisibilityConverter}}}" />
```

## 2. `IfExtension`

Using the `Conditional expression` in XAML.

```xml
<Button Command="{markupEx:If
                 {Binding BoolProperty},
                 {Binding OkCommand},
                 {Binding CancelCommand}}" />
```

## 3. `I18nExtension`

Dynamically switch languages without restarting the app.

```xml
<TextBlock Text="{markupEx:I18n {x:Static language:UiStrings.MainWindow_Title}}" />
```

## 4. `StylesExtension` (In Progress)

```xml
<Button Style="{Styles {StaticResource FlatButtonStyle},
                       {StaticResource AnimationStyle}
                       ...}" />
```

## 5. `SwitchExtension` (TBD)

```xml
<UserControl Width="{Switch {Binding Path=ActualWidth, ElementName=MainWindow},
                    {Case 400, 0},
                    {Case 600, 100},
                    {Case 1000, 150}}" />
```

```xml
<Image Source="{Switch {Binding FileType},
               {Case {x:Static res:FileType.Music}, {StaticResource MusicIcon}},
               {Case {x:Static res:FileType.Video}, {StaticResource VideoIcon}},
               {Case {x:Static res:FileType.Picture}, {StaticResource PictureIcon}},
               ...}" />
```
