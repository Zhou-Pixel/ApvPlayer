namespace ApvPlayer.EventArgs;

public class MpvPropertyChangedEventArgs : System.EventArgs
{
    public MpvPropertyChangedEventArgs(object newValue, string mpvPropertyName, bool spontaneous = true, string? propertyName = null)
    {
        NewValue = newValue;
        PropertyName = propertyName;
        MpvPropertyName = mpvPropertyName;
        Spontaneous = spontaneous;
    }

    public object NewValue { get; }
    
    public string MpvPropertyName { get; }
    public string? PropertyName { get; }

    public bool Spontaneous { get; }
}