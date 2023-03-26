using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ApvPlayer.Attributes;
using ApvPlayer.EventArgs;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using ApvPlayer.FFI.LibMpv;
using Avalonia.Threading;

namespace ApvPlayer.Controls;

public sealed class OpenGlControl : OpenGlControlBase
{
    // public Mpv MpvHandle { set; get; }
    private bool _first = true;

    public Mpv? Handle { set; get; }

    private readonly Dictionary<string, object?> _valueCache = new();

    // public event Action<object, MpvPropertyChangedEventArgs>? MpvPropertyChanged; 
    //public event Action<object, MpvPropertyChangedEventArgs>? MpvPropertyChanged;
    public OpenGlControl()
    {

    }

    private void OpenGlControl_PointerPressed(object? sender, object e)
    {
        Console.WriteLine("presss");
    }



    // public void Seek(double pos)
    // {
    //     MpvHandle.CommandNode(new List<object>()
    //     {
    //         "seek",
    //         pos,
    //         "absolute"
    //     });
    // }
    //
    //[MpvProperty("pause")]
    //public bool Pause
    //{
    //    set
    //    {
    //        _valueCache["pause"] = value;
    //        Handle.SetProperty("pause", value);
    //    }
    //    get => (bool)Handle.GetProperty("pause");
    //}

    //[MpvProperty("ao-volume")]
    //public double Volume
    //{
    //    set
    //    {
    //        _valueCache["ao-volume"] = value;
    //        Handle.SetProperty("ao-volume", value);
    //    }
    //    get => (double)Handle.GetProperty("ao-volume");
    //}

    //[MpvProperty("duration")]
    //public double Duration => (double)Handle.GetProperty("duration");


    //[MpvProperty("time-pos")]
    //public double TimePos
    //{
    //    set
    //    {
    //        _valueCache["time-pos"] = value;
    //        Handle.SetProperty("time-pos", value);
    //    }
    //    get => (double)Handle.GetProperty("time-pos");
    //}

    public void CommandNode(params object[] cmd)
    {
        Handle?.CommandNode(cmd);
    }


    public void OpenFile(string url)
    {
        Handle?.CommandNode("loadfile", url);
    }

    //private void WakeUp(nint _)
    //{
    //    Task.Run(async () => //avoid block here 
    //    {
    //        while (true)
    //        {
    //            var evt = Handle.WaitEvent(0);
    //            if (evt.EventId == MpvEventId.MpvEventNone)
    //            {
    //                break;
    //            }

    //            if (evt.EventId == MpvEventId.MpvEventPropertyChange)
    //            {
    //                MpvEventProperty prop = Marshal.PtrToStructure<MpvEventProperty>(evt.Data);
    //                string name = Marshal.PtrToStringAnsi(prop.Name) ?? string.Empty;
    //                var data = prop.TakeData();
    //                if (data == null)
    //                {
    //                    continue;
    //                }
    //                bool sp;
    //                if (_valueCache.ContainsKey(name) && data.Equals(_valueCache[name]))
    //                    sp = false;
    //                else
    //                    sp = true;
    //                await Dispatcher.UIThread.InvokeAsync(() =>
    //                {
    //                    MpvPropertyChanged?.Invoke(this, new MpvPropertyChangedEventArgs(data, name, sp));
    //                });
    //                //if (name == "time-pos")
    //                //{
    //                //    if (prop.Data == nint.Zero)
    //                //    {
    //                //        Console.WriteLine("continuw");
    //                //        continue;
    //                //    }
    //                //    var ret = new double[1];
    //                //    Marshal.Copy(prop.Data, ret, 0, 1);
    //                //    await Dispatcher.UIThread.InvokeAsync(() =>
    //                //    {
    //                //    });
    //                //}
    //                //else if (name == "duration")
    //                //{

    //                //    if (prop.Data == nint.Zero)
    //                //    {
    //                //        Console.WriteLine("continuw");
    //                //        continue;
    //                //    }

    //                //    var ret = new double[1];
    //                //    Marshal.Copy(prop.Data, ret, 0, 1);
    //                //    await Dispatcher.UIThread.InvokeAsync(() =>
    //                //    {
    //                //    });
    //                //}
    //                //else if (name == "ao-volume")
    //                //{
    //                //    var ret = new double[1];
    //                //    Marshal.Copy(prop.Data, ret, 0, 1);
    //                //    await Dispatcher.UIThread.InvokeAsync(() =>
    //                //    {
    //                //    });
    //                //}

    //            }

    //        }
    //    });

    //}

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        double scaling = VisualRoot?.RenderScaling ?? 1.0;
        var fbo = new MpvOpenglFbo()
        {
            Fbo = fb,
            W = (int)(Bounds.Width * scaling ),
            H = (int)(Bounds.Height * scaling ),
            InternalFormat = 0
        };
        var parameters = new Dictionary<MpvRenderParamType, object>()
        {
            { MpvRenderParamType.MpvRenderParamOpenglFbo, fbo},
            { MpvRenderParamType.MpvRenderParamFlipY, 1}
        };
        Handle?.RenderContextRender(parameters);
        // var flip = 1;
        // IntPtr fboPtr = Marshal.AllocHGlobal(Marshal.SizeOf(fbo));
        // Marshal.StructureToPtr(fbo, fboPtr, true);
        // unsafe
        // {
        //
        //     var _params = new MpvRenderParam[]
        //     {
        //         new MpvRenderParam()
        //         {
        //             type = MpvRenderParamType.MPV_RENDER_PARAM_OPENGL_FBO,
        //             data = fboPtr,
        //         },
        //         new MpvRenderParam()
        //         {
        //             type = MpvRenderParamType.MPV_RENDER_PARAM_FLIP_Y,
        //             data = new IntPtr(&flip)
        //         },
        //         new MpvRenderParam()
        //         {
        //             type = MpvRenderParamType.MPV_RENDER_PARAM_INVALID,
        //             data = IntPtr.Zero
        //         }
        //     };
        //     int code = Mpv.mpv_render_context_render(_mpvRenderCtx, Marshal.UnsafeAddrOfPinnedArrayElement(_params, 0));
        //     Console.WriteLine($"render result : {code}");
        // }
        // Marshal.FreeHGlobal(fboPtr);
        // Console.WriteLine("request update from render func");
        // this.RequestNextFrameRendering();//need this to update

    }


    private async void UpdateGl(nint _)
    {
        await Dispatcher.UIThread.InvokeAsync(RequestNextFrameRendering);
    }
    
    protected override void OnOpenGlInit(GlInterface gl)
    {
        
        if (!_first)
        {
            Console.WriteLine("init twice");
            return;
        }
        

        _first = false;
        MpvOpenglInitParams para = new()
        {
            MpvGetProcAddress = (_, name) => gl.GetProcAddress(name)
        };

        var parameters = new Dictionary<MpvRenderParamType, object>()
        {
            {MpvRenderParamType.MpvRenderParamApiType, "opengl"},
            {MpvRenderParamType.MpvRenderParamOpenglInitParams, para},
        };
        Handle.RenderContextCreate(parameters);
        Handle.RenderContextSetUpdateCallback(UpdateGl, nint.Zero);
        // IntPtr ptrs = Marshal.StringToHGlobalAnsi("opengl");
        // IntPtr paramsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(para));
        // Marshal.StructureToPtr(para, paramsPtr, true);
        // unsafe
        // {
        //
        //     var _params = new MpvRenderParam[]
        // {
        //     new MpvRenderParam()
        //     {
        //         type = MpvRenderParamType.MPV_RENDER_PARAM_API_TYPE,
        //         data = ptrs
        //     },
        //     new MpvRenderParam()
        //     {
        //         type = MpvRenderParamType.MPV_RENDER_PARAM_OPENGL_INIT_PARAMS,
        //         data = paramsPtr,
        //     },
        //     new MpvRenderParam()
        //     {
        //         type = MpvRenderParamType.MPV_RENDER_PARAM_INVALID,
        //         data = IntPtr.Zero
        //     }
        // };
        // var code = Mpv.mpv_render_context_create(out _mpvRenderCtx, _mpvHanle, Marshal.UnsafeAddrOfPinnedArrayElement(_params, 0));
        // }
        //
        // //DynamicMpv.RendeContextSetUpdateCallback(_mpvRenderCtx, _updateCallBack, IntPtr.Zero);
        // Mpv.mpv_render_context_set_update_callback(_mpvRenderCtx, _updateCallBack, IntPtr.Zero);
        // Marshal.FreeHGlobal(paramsPtr);
        //
        // Marshal.FreeHGlobal(ptrs);
    }

    // private void OnMpvPropertyChanged(object sender, MpvPropertyChangedEventArgs arg)
    // {
    //     MpvPropertyChanged?.Invoke(sender, arg);
    // }
}