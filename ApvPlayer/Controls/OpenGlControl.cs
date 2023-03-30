using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ApvPlayer.Attributes;
using ApvPlayer.EventArgs;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using ApvPlayer.FFI.LibMpv;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApvPlayer.Controls;

public sealed class OpenGlControl : OpenGlControlBase
{
    public static readonly StyledProperty<Mpv?> HandleProperty =
        AvaloniaProperty.Register<OpenGlControl, Mpv?>(nameof(Handle));

    private bool _first = true;

    public Mpv? Handle
    {
        set => SetValue(HandleProperty, value);
        get => GetValue(HandleProperty);
    }

    private readonly Dictionary<string, object?> _valueCache = new();

    public OpenGlControl()
    {
        PointerPressed += (sender, args) =>
        {
            Console.WriteLine("openglcontrol press");
        };
    }

    private void OpenGlControl_PointerPressed(object? sender, object e)
    {
        Console.WriteLine("presss");
    }

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
        Handle?.RenderContextCreate(parameters);
        Handle?.RenderContextSetUpdateCallback(UpdateGl, nint.Zero);
    }
}