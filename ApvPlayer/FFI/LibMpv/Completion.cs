using System;
using System.Runtime.CompilerServices;
using ApvPlayer.EventArgs;

namespace ApvPlayer.FFI.LibMpv;

public class Completion : INotifyCompletion
{
    public Completion(Mpv mpv)
    {
        mpv.MpvEventReceived += OnMpvEventReceived;
    }
    private Action? Continuation { get; set; }
    public bool IsCompleted { get; }
    public void GetResult() { }
    
    public void OnCompleted(Action continuation)
    {
        Continuation = continuation;
    }

    public void OnMpvEventReceived(object sender, MpvEventReceivedArgs args)
    {
    }
}

public class Completion<T> : INotifyCompletion where T : new()
{
    public bool IsCompleted { get; }

    public T GetResult()
    {
        return new T();
    }

    public void OnCompleted(Action continuation)
    {
        throw new NotImplementedException();
    }
}