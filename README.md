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
    <Switch Condition="{Binding SelectedViewName}">
        <Case Label="View1">
            <views:View1 />
        </Case>
        <Case Label="View2">
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
