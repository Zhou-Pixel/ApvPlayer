using System;
using Avalonia;
using Projektanker.Icons.Avalonia;
using Avalonia.Controls;

namespace ApvPlayer.Controls;

public class CheckBox : UserControl
{
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<CheckBox, bool>(nameof(IsChecked));
    
    public bool IsChecked 
    { 
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public CheckBox()
    {
        Content = new Icon()
        {
            Value = "fa-regular fa-square"
        };
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(IsChecked))
        {
            ((Icon)Content!).Value = (bool)e.NewValue! ? "fa-regular fa-square-check" : "fa-regular fa-square";
        }
    }
}