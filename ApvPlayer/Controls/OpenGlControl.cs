using System;
using System.Collections.Generic;
using ApvPlayer.FFI.LibMpv;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;

namespace ApvPlayer.Controls;

public sealed class OpenGlControl : OpenGlControlBase, ICustomHitTest
{
    public static readonly StyledProperty<Mpv> HandleProperty =
        AvaloniaProperty.Register<OpenGlControl, Mpv>(nameof(Handle));

    private bool _first = true;
    public required Mpv Handle
    {
        set => SetValue(HandleProperty, value);
        get => GetValue(HandleProperty);
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        double scaling = VisualRoot?.RenderScaling ?? 1.0;
        var fbo = new MpvOpenglFbo
        {
            Fbo = fb,
            W = (int)(Bounds.Width * scaling ),
            H = (int)(Bounds.Height * scaling ),
            InternalFormat = 0
        };
        var parameters = new Dictionary<MpvRenderParamType, object>
        {
            { MpvRenderParamType.MpvRenderParamOpenglFbo, fbo},
            { MpvRenderParamType.MpvRenderParamFlipY, 1}
        };
        Handle.RenderContextRender(parameters);
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

        var parameters = new Dictionary<MpvRenderParamType, object>
        {
            {MpvRenderParamType.MpvRenderParamApiType, "opengl"},
            {MpvRenderParamType.MpvRenderParamOpenglInitParams, para},
        };
        Handle.RenderContextCreate(parameters);
        Handle.RenderContextSetUpdateCallback(UpdateGl, nint.Zero);
    }


    public bool HitTest(Point point) => true;
}