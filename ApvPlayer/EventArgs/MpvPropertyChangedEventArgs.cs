namespace ApvPlayer.EventArgs;

public class MpvPropertyChangedEventArgs : System.EventArgs
{
    public MpvPropertyChangedEventArgs(object newValue, string mpvPropertyName, bool fromMpv = true, string? propertyName = null)
    {
        NewValue = newValue;
        PropertyName = propertyName;
        MpvPropertyName = mpvPropertyName;
        FromMpv = fromMpv;
    }

    public object NewValue { get; }
    
    public string MpvPropertyName { get; }
    public string? PropertyName { get; }

    public bool FromMpv { get; }
}