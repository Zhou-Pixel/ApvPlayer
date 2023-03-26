using System;
using System.Runtime.InteropServices;
using ApvPlayer.FFI.LibraryLoaders;
using LibMpv.LibraryLoaders;

namespace ApvPlayer.FFI.LibMpv;

public class MpvFunctions
{
    
    private readonly ILibraryLoader _libraryLoader;
    private static string _mpvLibraryPath = string.Empty;
    private static readonly Lazy<MpvFunctions> Lazy =
        new Lazy<MpvFunctions>(() => new MpvFunctions());
    
    public static MpvFunctions Instance => Lazy.Value;

    public static void Setup(string mpvPath)
    {
        _mpvLibraryPath = mpvPath;
    }

    private MpvFunctions()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _libraryLoader = new LinuxLibraryLoader();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _libraryLoader = new WindowsLibraryLoader();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _libraryLoader = new MacLibraryLoader();
        }
        else
        {
            throw new NotSupportedException("current os is not support");
        }

        _libraryLoader.LoadDynamicLibrary(_mpvLibraryPath);
        
        Create = GetDelegate<MpvCreate>("mpv_create");
        Initialize = GetDelegate<MpvInitialize>("mpv_initialize");
        SetOptionString = GetDelegate<MpvSetOptionString>("mpv_set_option_string");
        SetWakeupCallback = GetDelegate<MpvSetWakeupCallback>("mpv_set_wakeup_callback");
        WaitEvent = GetDelegate<MpvWaitEvent>("mpv_wait_event");
        RenderContextCreate = GetDelegate<MpvRenderContextCreate>("mpv_render_context_create");
        RenderContextSetUpdateCallback = GetDelegate<MpvRenderContextSetUpdateCallback>("mpv_render_context_set_update_callback");
        RenderContextRender = GetDelegate<MpvRenderContextRender>("mpv_render_context_render");
        CommandNode = GetDelegate<MpvCommandNode>("mpv_command_node");
        ErrorString = GetDelegate<MpvErrorString>("mpv_error_string");
        RenderContextFree = GetDelegate<MpvRenderContextFree>("mpv_render_context_free");
        TerminateDestroy = GetDelegate<MpvTerminateDestroy>("mpv_terminate_destroy");
        ObserveProperty = GetDelegate<MpvObserveProperty>("mpv_observe_property");
        GetProperty = GetDelegate<MpvGetProperty>("mpv_get_property");
        FreeNodeContents = GetDelegate<MpvFreeNodeContents>("mpv_free_node_contents");
        SetProperty = GetDelegate<MpvSetProperty>("mpv_set_property");
    }


    private TDelegate GetDelegate<TDelegate>(string path)
    {
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(_libraryLoader.GetProcAddress(path));
    }

    public MpvCreate Create { get; }

    // int mpv_initialize(mpv_handle *ctx);
    public MpvInitialize Initialize { get; }
    
    // int mpv_set_option_string(mpv_handle *ctx, const char *name, const char *data);
    public MpvSetOptionString SetOptionString { get; }

    // void mpv_set_wakeup_callback(mpv_handle *ctx, void (*cb)(void *d), void *d);
    public MpvSetWakeupCallback SetWakeupCallback { get; }

    // mpv_event *mpv_wait_event(mpv_handle *ctx, double timeout);
    public MpvWaitEvent WaitEvent { get; }

    // int mpv_render_context_create(mpv_render_context **res, mpv_handle *mpv, mpv_render_param *params);
    public MpvRenderContextCreate RenderContextCreate { get; }


    // void mpv_render_context_set_update_callback(mpv_render_context *ctx, mpv_render_update_fn callback, void *callback_ctx);
    public MpvRenderContextSetUpdateCallback RenderContextSetUpdateCallback { get; }

    // int mpv_render_context_render(mpv_render_context *ctx, mpv_render_param *params);
    public MpvRenderContextRender RenderContextRender { get; }

    // int mpv_command_node(mpv_handle *ctx, mpv_node *args, mpv_node *result);
    public MpvCommandNode CommandNode { get; }
    
    //const char *mpv_error_string(int error);
    public MpvErrorString ErrorString { get; }
    
    public MpvRenderContextFree RenderContextFree { get; }
    
    public MpvTerminateDestroy TerminateDestroy { get; }
    
    public MpvObserveProperty ObserveProperty { get; }
    
    public MpvGetProperty GetProperty { get; }
    
    public MpvFreeNodeContents FreeNodeContents { get; }
    
    
    public MpvSetProperty SetProperty { get; }
     
}