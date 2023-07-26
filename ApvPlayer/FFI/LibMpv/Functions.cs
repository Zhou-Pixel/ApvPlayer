using System;
using System.Runtime.InteropServices;
using ApvPlayer.FFI.LibraryLoaders;

namespace ApvPlayer.FFI.LibMpv;

public class Functions
{
    
    private readonly ILibraryLoader _libraryLoader;
    private static string _mpvLibraryPath = string.Empty;
    private static readonly Lazy<Functions> Lazy =
        new Lazy<Functions>(() => new Functions());
    
    public static Functions Instance => Lazy.Value;

    public static void Setup(string mpvPath)
    {
        _mpvLibraryPath = mpvPath;
    }

    private Functions()
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
        
        Create = GetDelegate<Create>("mpv_create");
        Initialize = GetDelegate<Initialize>("mpv_initialize");
        SetOptionString = GetDelegate<SetOptionString>("mpv_set_option_string");
        SetWakeupCallback = GetDelegate<SetWakeupCallback>("mpv_set_wakeup_callback");
        WaitEvent = GetDelegate<WaitEvent>("mpv_wait_event");
        RenderContextCreate = GetDelegate<RenderContextCreate>("mpv_render_context_create");
        RenderContextSetUpdateCallback = GetDelegate<RenderContextSetUpdateCallback>("mpv_render_context_set_update_callback");
        RenderContextRender = GetDelegate<RenderContextRender>("mpv_render_context_render");
        CommandNode = GetDelegate<CommandNode>("mpv_command_node");
        Command = GetDelegate<Command>("mpv_command");
        ErrorString = GetDelegate<ErrorString>("mpv_error_string");
        RenderContextFree = GetDelegate<RenderContextFree>("mpv_render_context_free");
        TerminateDestroy = GetDelegate<TerminateDestroy>("mpv_terminate_destroy");
        ObserveProperty = GetDelegate<ObserveProperty>("mpv_observe_property");
        GetProperty = GetDelegate<GetProperty>("mpv_get_property");
        FreeNodeContents = GetDelegate<FreeNodeContents>("mpv_free_node_contents");
        SetProperty = GetDelegate<SetProperty>("mpv_set_property");
        CommandAsync = GetDelegate<CommandAsync>("mpv_command_async");
        CommandNodeAsync = GetDelegate<CommandNodeAsync>("mpv_command_node_async");
        SetPropertyAsync = GetDelegate<SetPropertyAsync>("mpv_set_property_async");
    }

    ~Functions()
    {
        _libraryLoader.Dispose();
    }

    private TDelegate GetDelegate<TDelegate>(string path)
    {
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(_libraryLoader.GetProcAddress(path));
    }

    public Create Create { get; }

    // int mpv_initialize(mpv_handle *ctx);
    public Initialize Initialize { get; }
    
    // int mpv_set_option_string(mpv_handle *ctx, const char *name, const char *data);
    public SetOptionString SetOptionString { get; }

    // void mpv_set_wakeup_callback(mpv_handle *ctx, void (*cb)(void *d), void *d);
    public SetWakeupCallback SetWakeupCallback { get; }

    // mpv_event *mpv_wait_event(mpv_handle *ctx, double timeout);
    public WaitEvent WaitEvent { get; }

    // int mpv_render_context_create(mpv_render_context **res, mpv_handle *mpv, mpv_render_param *params);
    public RenderContextCreate RenderContextCreate { get; }


    // void mpv_render_context_set_update_callback(mpv_render_context *ctx, mpv_render_update_fn callback, void *callback_ctx);
    public RenderContextSetUpdateCallback RenderContextSetUpdateCallback { get; }

    // int mpv_render_context_render(mpv_render_context *ctx, mpv_render_param *params);
    public RenderContextRender RenderContextRender { get; }

    // int mpv_command_node(mpv_handle *ctx, mpv_node *args, mpv_node *result);
    public CommandNode CommandNode { get; }

    public Command Command { get; }
    
    //const char *mpv_error_string(int error);
    public ErrorString ErrorString { get; }
    
    public RenderContextFree RenderContextFree { get; }
    
    public TerminateDestroy TerminateDestroy { get; }
    
    public ObserveProperty ObserveProperty { get; }
    
    public GetProperty GetProperty { get; }
    
    public FreeNodeContents FreeNodeContents { get; }
    
    
    public SetProperty SetProperty { get; }
    
    public CommandAsync CommandAsync { get; }
    
    public CommandNodeAsync CommandNodeAsync { get; }
    
    public SetPropertyAsync  SetPropertyAsync { get; }
     
}