using System.Runtime.InteropServices;

namespace ApvPlayer.FFI.LibMpv;

// mpv_handle *mpv_create(void);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate nint Create();

// int mpv_initialize(mpv_handle *ctx);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int Initialize(nint ctx);
    
// int mpv_set_option_string(mpv_handle *ctx, const char *name, const char *data);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int SetOptionString(nint ctx, nint name, nint data);

// void mpv_set_wakeup_callback(mpv_handle *ctx, void (*cb)(void *d), void *d);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void SetWakeupCallback(nint ctx, [MarshalAs(UnmanagedType.FunctionPtr)] WakeupCallback cb, nint data);

// mpv_event *mpv_wait_event(mpv_handle *ctx, double timeout);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate nint WaitEvent(nint ctx, double timeout);

// int mpv_render_context_create(mpv_render_context **res, mpv_handle *mpv, mpv_render_param *params);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int RenderContextCreate(nint outCtx, nint mpv, nint paras);


// void mpv_render_context_set_update_callback(mpv_render_context *ctx, mpv_render_update_fn callback, void *callback_ctx);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void RenderContextSetUpdateCallback(nint renderPtrctx,
	[MarshalAs(UnmanagedType.FunctionPtr)] RenderContextUpdateCallback cb,
	nint data);

// int mpv_render_context_render(mpv_render_context *ctx, mpv_render_param *params);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int RenderContextRender(nint ctx, nint paras);

// int mpv_command_node(mpv_handle *ctx, mpv_node *args, mpv_node *result);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int CommandNode(nint mpv, nint args, nint res);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate nint ErrorString(int error);

//void mpv_render_context_free(mpv_render_context *ctx);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void RenderContextFree(nint ctx);

//void mpv_terminate_destroy(mpv_handle *ctx);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void TerminateDestroy(nint ctx);

//int mpv_observe_property(mpv_handle *mpv, uint64_t reply_userdata, const char *name, mpv_format format);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int ObserveProperty(nint mpv, ulong replyUserdata, nint name, Format format);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void WakeupCallback(nint data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void RenderContextUpdateCallback(nint data);

// int mpv_get_property(mpv_handle *ctx, const char *name, mpv_format format, void *data);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int GetProperty(nint ctx, nint name, Format format, nint data);


// void mpv_free_node_contents(mpv_node *node);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void FreeNodeContents(nint node);

// int mpv_set_property(mpv_handle *ctx, const char *name, mpv_format format, void *data);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int SetProperty(nint ctx, nint name, Format format, nint data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int Command(nint ctx, nint args);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int CommandAsync(nint ctx, ulong replyUserData, nint args);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int CommandNodeAsync(nint ctx, ulong replyUserData, nint args);


[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int SetPropertyAsync(nint ctx, ulong replyUserdata, nint name, Format format, nint data);