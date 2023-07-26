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

    public OpenGlControl()
    {
        Console.WriteLine("new OpenGlControl");
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        double scaling = VisualRoot?.RenderScaling ?? 1.0;
        var fbo = new OpenglFbo
        {
            Fbo = fb,
            W = (int)(Bounds.Width * scaling),
            H = (int)(Bounds.Height * scaling),
            InternalFormat = 0
        };
        var parameters = new Dictionary<RenderParamType, object>
        {
            { RenderParamType.OpenglFbo, fbo },
            { RenderParamType.FlipY, 1 }
        };
        Handle.RenderContextRender(parameters);
    }


    private async void UpdateGl(nint _)
    {
        await Dispatcher.UIThread.InvokeAsync(RequestNextFrameRendering);
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        // bug due to avalonia https://github.com/AvaloniaUI/Avalonia/issues/10371
        if (!_first)
        {
            Console.WriteLine("init twice");
            return;
        }


        _first = false;
        OpenglInitParams para = new()
        {
            MpvGetProcAddress = (_, name) => gl.GetProcAddress(name)
        };

        var parameters = new Dictionary<RenderParamType, object>
        {
            { RenderParamType.ApiType, "opengl" },
            { RenderParamType.OpenglInitParams, para },
        };
        Handle.RenderContextCreate(parameters);
        Handle.RenderContextSetUpdateCallback(UpdateGl, nint.Zero);
    }


    public bool HitTest(Point point) => true;
}