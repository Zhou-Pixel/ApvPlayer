using System;
using Avalonia;
using Projektanker.Icons.Avalonia;
using Avalonia.Controls;

namespace ApvPlayer.Controls;

public class CheckBox : UserControl
{
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<CheckBox, bool>(nameof(IsChecked), false);
    
    public static readonly StyledProperty<string> CheckedIconProperty =
        AvaloniaProperty.Register<CheckBox, string>(nameof(CheckedIcon), "fa-regular fa-square-check");
    
    public static readonly StyledProperty<string> UnCheckedIconProperty =
        AvaloniaProperty.Register<CheckBox, string>(nameof(UnCheckedIcon), "fa-regular fa-square");
    
    public bool IsChecked 
    { 
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public string CheckedIcon 
    { 
        set => SetValue(CheckedIconProperty, value);
        get => GetValue(CheckedIconProperty); 
    }


    public string UnCheckedIcon 
    { 
        set => SetValue(UnCheckedIconProperty, value);
        get => GetValue(UnCheckedIconProperty);
    }
    

    public CheckBox()
    {
        Content = new Icon()
        {
            Value = UnCheckedIcon
        };
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        ((Icon)Content!).Value = e.Property.Name switch
        {
            nameof(IsChecked) => (bool)e.NewValue! ? CheckedIcon : UnCheckedIcon,
            nameof(UnCheckedIcon) or nameof(CheckedIcon) => IsChecked ? CheckedIcon : UnCheckedIcon,
            _ => ((Icon)Content!).Value
        };
    }
}