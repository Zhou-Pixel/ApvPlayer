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

    // public event Action<object, MpvPropertyChangedEventArgs>? MpvPropertyChanged; 
    //public event Action<object, MpvPropertyChangedEventArgs>? MpvPropertyChanged;
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
        Handle?.RenderContextCreate(parameters);
        Handle?.RenderContextSetUpdateCallback(UpdateGl, nint.Zero);
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



    ///// <summary>
    ///// Defines the <see cref="Background"/> property.
    ///// </summary>
    //public static readonly StyledProperty<IBrush?> BackgroundProperty =
    //    Border.BackgroundProperty.AddOwner<TemplatedControl>();

    ///// <summary>
    ///// Defines the <see cref="BorderBrush"/> property.
    ///// </summary>
    //public static readonly StyledProperty<IBrush?> BorderBrushProperty =
    //    Border.BorderBrushProperty.AddOwner<TemplatedControl>();

    ///// <summary>
    ///// Defines the <see cref="BorderThickness"/> property.
    ///// </summary>
    //public static readonly StyledProperty<Thickness> BorderThicknessProperty =
    //    Border.BorderThicknessProperty.AddOwner<TemplatedControl>();

    ///// <summary>
    ///// Defines the <see cref="CornerRadius"/> property.
    ///// </summary>
    //public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
    //    Border.CornerRadiusProperty.AddOwner<TemplatedControl>();

    ///// <summary>
    ///// Defines the <see cref="FontFamily"/> property.
    ///// </summary>
    //public static readonly StyledProperty<FontFamily> FontFamilyProperty =
    //    TextElement.FontFamilyProperty.AddOwner<TemplatedControl>();

    ///// <summary>
    ///// Defines the <see cref="FontSize"/> property.
    ///// </summary>
    //public static readonly StyledProperty<double> FontSizeProperty =
    //    TextElement.FontSizeProperty.AddOwner<TemplatedControl>();

    ///// <summary>
    ///// Defines the <see cref="FontStyle"/> property.
    ///// </summary>
    //public static readonly StyledProperty<FontStyle> FontStyleProperty =
    //    TextElement.FontStyleProperty.AddOwner<TemplatedControl>();

    ///// <summary>
    ///// Defines the <see cref="FontWeight"/> property.
    ///// </summary>
    //public static readonly StyledProperty<FontWeight> FontWeightProperty =
    //    TextElement.FontWeightProperty.AddOwner<TemplatedControl>();

    ///// <summary>
    ///// Defines the <see cref="FontWeight"/> property.
    ///// </summary>
    //public static readonly StyledProperty<FontStretch> FontStretchProperty =
    //    TextElement.FontStretchProperty.AddOwner<TemplatedControl>();

    ///// <summary>
    ///// Defines the <see cref="Foreground"/> property.
    ///// </summary>
    //public static readonly StyledProperty<IBrush?> ForegroundProperty =
    //    TextElement.ForegroundProperty.AddOwner<TemplatedControl>();

    ///// <summary>
    ///// Defines the <see cref="Padding"/> property.
    ///// </summary>
    //public static readonly StyledProperty<Thickness> PaddingProperty =
    //    Decorator.PaddingProperty.AddOwner<TemplatedControl>();

    ///// <summary>
    ///// Defines the <see cref="Template"/> property.
    ///// </summary>
    //public static readonly StyledProperty<IControlTemplate?> TemplateProperty =
    //    AvaloniaProperty.Register<TemplatedControl, IControlTemplate?>(nameof(Template));

    ///// <summary>
    ///// Defines the IsTemplateFocusTarget attached property.
    ///// </summary>
    //public static readonly AttachedProperty<bool> IsTemplateFocusTargetProperty =
    //    AvaloniaProperty.RegisterAttached<TemplatedControl, Control, bool>("IsTemplateFocusTarget");

    ///// <summary>
    ///// Defines the <see cref="TemplateApplied"/> routed event.
    ///// </summary>
    //public static readonly RoutedEvent<TemplateAppliedEventArgs> TemplateAppliedEvent =
    //    RoutedEvent.Register<TemplatedControl, TemplateAppliedEventArgs>(
    //        nameof(TemplateApplied),
    //        RoutingStrategies.Direct);

    //private IControlTemplate? _appliedTemplate;

    ///// <summary>
    ///// Initializes static members of the <see cref="TemplatedControl"/> class.
    ///// </summary>
    //static OpenGlControl()
    //{
    //    ClipToBoundsProperty.OverrideDefaultValue<TemplatedControl>(true);
    //    TemplateProperty.Changed.AddClassHandler<TemplatedControl>((x, e) => x.OnTemplateChanged(e));
    //}

    ///// <summary>
    ///// Raised when the control's template is applied.
    ///// </summary>
    //public event EventHandler<TemplateAppliedEventArgs>? TemplateApplied
    //{
    //    add { AddHandler(TemplateAppliedEvent, value); }
    //    remove { RemoveHandler(TemplateAppliedEvent, value); }
    //}

    ///// <summary>
    ///// Gets or sets the brush used to draw the control's background.
    ///// </summary>
    //public IBrush? Background
    //{
    //    get { return GetValue(BackgroundProperty); }
    //    set { SetValue(BackgroundProperty, value); }
    //}

    ///// <summary>
    ///// Gets or sets the brush used to draw the control's border.
    ///// </summary>
    //public IBrush? BorderBrush
    //{
    //    get { return GetValue(BorderBrushProperty); }
    //    set { SetValue(BorderBrushProperty, value); }
    //}

    ///// <summary>
    ///// Gets or sets the thickness of the control's border.
    ///// </summary>
    //public Thickness BorderThickness
    //{
    //    get { return GetValue(BorderThicknessProperty); }
    //    set { SetValue(BorderThicknessProperty, value); }
    //}

    ///// <summary>
    ///// Gets or sets the radius of the border rounded corners.
    ///// </summary>
    //public CornerRadius CornerRadius
    //{
    //    get { return GetValue(CornerRadiusProperty); }
    //    set { SetValue(CornerRadiusProperty, value); }
    //}

    ///// <summary>
    ///// Gets or sets the font family used to draw the control's text.
    ///// </summary>
    //public FontFamily FontFamily
    //{
    //    get { return GetValue(FontFamilyProperty); }
    //    set { SetValue(FontFamilyProperty, value); }
    //}

    ///// <summary>
    ///// Gets or sets the size of the control's text in points.
    ///// </summary>
    //public double FontSize
    //{
    //    get { return GetValue(FontSizeProperty); }
    //    set { SetValue(FontSizeProperty, value); }
    //}

    ///// <summary>
    ///// Gets or sets the font style used to draw the control's text.
    ///// </summary>
    //public FontStyle FontStyle
    //{
    //    get { return GetValue(FontStyleProperty); }
    //    set { SetValue(FontStyleProperty, value); }
    //}

    ///// <summary>
    ///// Gets or sets the font weight used to draw the control's text.
    ///// </summary>
    //public FontWeight FontWeight
    //{
    //    get { return GetValue(FontWeightProperty); }
    //    set { SetValue(FontWeightProperty, value); }
    //}

    ///// <summary>
    ///// Gets or sets the font stretch used to draw the control's text.
    ///// </summary>
    //public FontStretch FontStretch
    //{
    //    get { return GetValue(FontStretchProperty); }
    //    set { SetValue(FontStretchProperty, value); }
    //}

    ///// <summary>
    ///// Gets or sets the brush used to draw the control's text and other foreground elements.
    ///// </summary>
    //public IBrush? Foreground
    //{
    //    get { return GetValue(ForegroundProperty); }
    //    set { SetValue(ForegroundProperty, value); }
    //}

    ///// <summary>
    ///// Gets or sets the padding placed between the border of the control and its content.
    ///// </summary>
    //public Thickness Padding
    //{
    //    get { return GetValue(PaddingProperty); }
    //    set { SetValue(PaddingProperty, value); }
    //}

    ///// <summary>
    ///// Gets or sets the template that defines the control's appearance.
    ///// </summary>
    //public IControlTemplate? Template
    //{
    //    get { return GetValue(TemplateProperty); }
    //    set { SetValue(TemplateProperty, value); }
    //}

    ///// <summary>
    ///// Gets the value of the IsTemplateFocusTargetProperty attached property on a control.
    ///// </summary>
    ///// <param name="control">The control.</param>
    ///// <returns>The property value.</returns>
    ///// <see cref="SetIsTemplateFocusTarget(Control, bool)"/>
    //public static bool GetIsTemplateFocusTarget(Control control)
    //{
    //    return control.GetValue(IsTemplateFocusTargetProperty);
    //}

    ///// <summary>
    ///// Sets the value of the IsTemplateFocusTargetProperty attached property on a control.
    ///// </summary>
    ///// <param name="control">The control.</param>
    ///// <param name="value">The property value.</param>
    ///// <remarks>
    ///// When a control is navigated to using the keyboard, a focus adorner is shown - usually
    ///// around the control itself. However if the TemplatedControl.IsTemplateFocusTarget 
    ///// attached property is set to true on an element in the control template, then the focus
    ///// adorner will be shown around that control instead.
    ///// </remarks>
    //public static void SetIsTemplateFocusTarget(Control control, bool value)
    //{
    //    control.SetValue(IsTemplateFocusTargetProperty, value);
    //}

    ///// <inheritdoc/>
    //public sealed override void ApplyTemplate()
    //{
    //    var template = Template;
    //    var logical = (ILogical)this;

    //    // Apply the template if it is not the same as the template already applied - except
    //    // for in the case that the template is null and we're not attached to the logical 
    //    // tree. In that case, the template has probably been cleared because the style setting
    //    // the template has been detached, so we want to wait until it's re-attached to the 
    //    // logical tree as if it's re-attached to the same tree the template will be the same
    //    // and we don't need to do anything.
    //    if (_appliedTemplate != template && (template != null || logical.IsAttachedToLogicalTree))
    //    {
    //        if (VisualChildren.Count > 0)
    //        {
    //            foreach (var child in this.GetTemplateChildren())
    //            {
    //                child.SetValue(TemplatedParentProperty, null);
    //                ((ISetLogicalParent)child).SetParent(null);
    //            }

    //            VisualChildren.Clear();
    //        }

    //        if (template != null)
    //        {
    //            Logger.TryGet(LogEventLevel.Verbose, LogArea.Control)?.Log(this, "Creating control template");

    //            var (child, nameScope) = template.Build(this);
    //            ApplyTemplatedParent(child, this);
    //            ((ISetLogicalParent)child).SetParent(this);
    //            VisualChildren.Add(child);

    //            // Existing code kinda expect to see a NameScope even if it's empty
    //            if (nameScope == null)
    //            {
    //                nameScope = new NameScope();
    //            }

    //            var e = new TemplateAppliedEventArgs(nameScope);
    //            OnApplyTemplate(e);
    //            RaiseEvent(e);
    //        }

    //        _appliedTemplate = template;
    //    }
    //}

    ///// <inheritdoc/>
    //protected override Control GetTemplateFocusTarget()
    //{
    //    foreach (Control child in this.GetTemplateChildren())
    //    {
    //        if (GetIsTemplateFocusTarget(child))
    //        {
    //            return child;
    //        }
    //    }

    //    return this;
    //}

    //protected sealed override void NotifyChildResourcesChanged(ResourcesChangedEventArgs e)
    //{
    //    var count = VisualChildren.Count;

    //    for (var i = 0; i < count; ++i)
    //    {
    //        if (VisualChildren[i] is ILogical logical)
    //        {
    //            logical.NotifyResourcesChanged(e);
    //        }
    //    }

    //    base.NotifyChildResourcesChanged(e);
    //}

    ///// <inheritdoc/>
    //protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    //{
    //    if (VisualChildren.Count > 0)
    //    {
    //        ((ILogical)VisualChildren[0]).NotifyAttachedToLogicalTree(e);
    //    }

    //    base.OnAttachedToLogicalTree(e);
    //}

    ///// <inheritdoc/>
    //protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    //{
    //    if (VisualChildren.Count > 0)
    //    {
    //        ((ILogical)VisualChildren[0]).NotifyDetachedFromLogicalTree(e);
    //    }

    //    base.OnDetachedFromLogicalTree(e);
    //}

    ///// <summary>
    ///// Called when the control's template is applied.
    ///// In simple terms, this means the method is called just before the control is displayed.
    ///// </summary>
    ///// <param name="e">The event args.</param>
    //protected virtual void OnApplyTemplate(TemplateAppliedEventArgs e)
    //{
    //}

    ///// <summary>
    ///// Called when the <see cref="Template"/> property changes.
    ///// </summary>
    ///// <param name="e">The event args.</param>
    //protected virtual void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    //{
    //    InvalidateMeasure();
    //}

    ///// <summary>
    ///// Sets the TemplatedParent property for the created template children.
    ///// </summary>
    ///// <param name="control">The control.</param>
    ///// <param name="templatedParent">The templated parent to apply.</param>
    //internal static void ApplyTemplatedParent(StyledElement control, AvaloniaObject? templatedParent)
    //{
    //    control.SetValue(TemplatedParentProperty, templatedParent);

    //    var children = control.LogicalChildren;
    //    var count = children.Count;

    //    for (var i = 0; i < count; i++)
    //    {
    //        if (children[i] is StyledElement child && child.TemplatedParent is null)
    //        {
    //            ApplyTemplatedParent(child, templatedParent);
    //        }
    //    }
    //}

    //private protected override void OnControlThemeChanged()
    //{
    //    base.OnControlThemeChanged();

    //    var count = VisualChildren.Count;
    //    for (var i = 0; i < count; ++i)
    //    {
    //        if (VisualChildren[i] is StyledElement child &&
    //            child.TemplatedParent == this)
    //        {
    //            child.OnTemplatedParentControlThemeChanged();
    //        }
    //    }
    //}

    //internal override void OnTemplatedParentControlThemeChanged()
    //{
    //    base.OnTemplatedParentControlThemeChanged();

    //    var count = VisualChildren.Count;
    //    var templatedParent = TemplatedParent;

    //    for (var i = 0; i < count; ++i)
    //    {
    //        if (VisualChildren[i] is TemplatedControl child &&
    //            child.TemplatedParent == templatedParent)
    //        {
    //            child.OnTemplatedParentControlThemeChanged();
    //        }
    //    }
    //}


}