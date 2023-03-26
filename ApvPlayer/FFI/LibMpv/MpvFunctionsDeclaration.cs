using System.Runtime.InteropServices;

namespace ApvPlayer.FFI.LibMpv;

// mpv_handle *mpv_create(void);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate nint MpvCreate();

// int mpv_initialize(mpv_handle *ctx);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int MpvInitialize(nint ctx);
    
// int mpv_set_option_string(mpv_handle *ctx, const char *name, const char *data);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int MpvSetOptionString(nint ctx, nint name, nint data);

// void mpv_set_wakeup_callback(mpv_handle *ctx, void (*cb)(void *d), void *d);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void MpvSetWakeupCallback(nint ctx, nint cb, nint data);

// mpv_event *mpv_wait_event(mpv_handle *ctx, double timeout);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate nint MpvWaitEvent(nint ctx, double timeout);

// int mpv_render_context_create(mpv_render_context **res, mpv_handle *mpv, mpv_render_param *params);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int MpvRenderContextCreate(nint outCtx, nint mpv, nint paras);


// void mpv_render_context_set_update_callback(mpv_render_context *ctx, mpv_render_update_fn callback, void *callback_ctx);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void MpvRenderContextSetUpdateCallback(nint renderPtrctx,
	nint cb,
	nint data);

// int mpv_render_context_render(mpv_render_context *ctx, mpv_render_param *params);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int MpvRenderContextRender(nint ctx, nint paras);

// int mpv_command_node(mpv_handle *ctx, mpv_node *args, mpv_node *result);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int MpvCommandNode(nint mpv, nint args, nint res);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate nint MpvErrorString(int error);

//void mpv_render_context_free(mpv_render_context *ctx);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void MpvRenderContextFree(nint ctx);

//void mpv_terminate_destroy(mpv_handle *ctx);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void MpvTerminateDestroy(nint ctx);

//int mpv_observe_property(mpv_handle *mpv, uint64_t reply_userdata, const char *name, mpv_format format);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int MpvObserveProperty(nint mpv, ulong replyUserdata, nint name, MpvFormat format);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void MpvWakeupCallback(nint data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void MpvRenderContextUpdateCallback(nint data);

// int mpv_get_property(mpv_handle *ctx, const char *name, mpv_format format, void *data);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int MpvGetProperty(nint ctx, nint name, MpvFormat format, nint data);


// void mpv_free_node_contents(mpv_node *node);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void MpvFreeNodeContents(nint node);

// int mpv_set_property(mpv_handle *ctx, const char *name, mpv_format format, void *data);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int MpvSetProperty(nint ctx, nint name, MpvFormat format, nint data);
