using ApvPlayer.FFI.LibMpv;

namespace ApvPlayer.EventArgs;

public class MpvEventReceivedArgs : System.EventArgs
{
    public MpvEvent Event { get; }

    public MpvEventReceivedArgs(MpvEvent @event)
    {
        this.Event = @event;
    }
    
}