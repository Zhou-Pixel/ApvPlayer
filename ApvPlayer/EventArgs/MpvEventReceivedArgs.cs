using ApvPlayer.FFI.LibMpv;

namespace ApvPlayer.EventArgs;

public class MpvEventReceivedArgs : System.EventArgs
{
    public Event Event { get; }

    public MpvEventReceivedArgs(Event @event)
    {
        Event = @event;
    }
    
}