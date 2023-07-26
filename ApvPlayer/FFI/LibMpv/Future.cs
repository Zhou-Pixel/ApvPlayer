using System;
using System.Runtime.CompilerServices;
using ApvPlayer.EventArgs;

namespace ApvPlayer.FFI.LibMpv;

public class Future : INotifyCompletion
{
    public Future(Mpv mpv)
    {
        mpv.MpvEventReceived += OnMpvEventReceived;
    }
    
    public Future GetAwaiter()
    {
        return this;
    }

    public bool IsCompleted { get; private set; }
    public void GetResult() { }
    
    private Action? Continuation { get; set; }

    public required Func<Future, MpvEventReceivedArgs, bool> Handle { get; set; }

    public required Action Completed { get; set; }

    public void OnCompleted(Action continuation)
    {
        Completed.Invoke();
        Continuation = continuation;
    }

    private void OnMpvEventReceived(object sender, MpvEventReceivedArgs args)
    {
        if (!Handle.Invoke(this, args)) return;
        IsCompleted = true;
        Continuation?.Invoke();
    }
}


public class Future<T> : Future
{
    public Future(Mpv mpv) : base(mpv)
    {
    }
    public new Future<T> GetAwaiter()
    {
        return this;
    }

    public new T GetResult() => Result!;
    
    public T? Result { set; get; }
    
}