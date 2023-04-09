using ApvPlayer.FFI.LibMpv;

namespace ApvPlayer.EventArgs;

public class MpvEventReceivedArgs : System.EventArgs
{
    public MpvEvent Evnet { get; }

    public MpvEventReceivedArgs(MpvEvent Evnet)
    {
        this.Evnet = Evnet;
    }
    
}