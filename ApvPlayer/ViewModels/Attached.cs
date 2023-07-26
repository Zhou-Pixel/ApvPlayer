using Avalonia;
using Avalonia.Controls;

namespace ApvPlayer.ViewModels;

public static class Attached
{
    public static readonly AttachedProperty<long?> IdProperty =
        AvaloniaProperty.RegisterAttached<MenuItem, Control, long?>("Id", defaultValue:null);

    public static long? GetId(Control element)
    {
        return element.GetValue(IdProperty);
    }

    public static void SetId(Control element, long? value)
    {
        element.SetValue(IdProperty, value);
    }
    
}