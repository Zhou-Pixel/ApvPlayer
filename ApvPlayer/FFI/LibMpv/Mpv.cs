using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ApvPlayer.Errors;
using ApvPlayer.EventArgs;
using Avalonia.Threading;

namespace ApvPlayer.FFI.LibMpv;

public class Mpv
{
    private static readonly MpvFunctions MpvFunctions = MpvFunctions.Instance;
    private readonly nint _mpvHandle = MpvFunctions.Create();
    private nint _mpvrender = nint.Zero;
    private RenderContextUpdateCallback? _renderContextUpdateCallback;
    private WakeupCallback _mpvWakeupCallback;
    private MpvGetProcAddressCallback? _mpvGetProcAddressCallback;

    private readonly Dictionary<string, object?> _valueCache = new();
    public event Action<object, MpvPropertyChangedEventArgs>? MpvPropertyChanged;
    public event Action<object, MpvEventReceivedArgs>? MpvEventReceived;
    public Mpv()
    {
        _mpvWakeupCallback = WakeupCallback;

        SetOptionString("terminal", "no");
        Initialize();
        ObserveProperty("duration", Format.Double);
        ObserveProperty("time-pos", Format.Double);
        SetWakeupCallback(_mpvWakeupCallback, nint.Zero);
    }

    ~Mpv()
    {
        if (_mpvrender != nint.Zero)
        {
            MpvFunctions.RenderContextFree(_mpvrender);
            _mpvrender = nint.Zero;
        }

        MpvFunctions.TerminateDestroy(_mpvHandle);
    }

    private void WakeupCallback(nint _)
    {
        Task.Run(async () => //avoid block here 
        {
            while (true)
            {
                var evt = WaitEvent(0);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    MpvEventReceived?.Invoke(this, new MpvEventReceivedArgs(evt));
                });
                if (evt.EventId == EventId.None)
                {
                    break;
                }

                switch (evt.EventId)
                {
                    case EventId.PropertyChange:
                    {
                        EventProperty prop = Marshal.PtrToStructure<EventProperty>(evt.Data);
                        string name = Marshal.PtrToStringAnsi(prop.Name) ?? string.Empty;
                        var data = prop.TakeData();
                        if (data == null)
                        {
                            continue;
                        }
                        bool fromMpv = true;
                        if (_valueCache.TryGetValue(name, out object? value) && data.Equals(value))
                            fromMpv = false;
                        else
                            fromMpv = true;
                        // Console.WriteLine($"property name {name} value {data} FromMpv {fromMpv}");
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            MpvPropertyChanged?.Invoke(this, new MpvPropertyChangedEventArgs(data, name, fromMpv));
                        });
                        //if (name == "time-pos")
                        //{
                        //    if (prop.Data == nint.Zero)
                        //    {
                        //        Console.WriteLine("continuw");
                        //        continue;
                        //    }
                        //    var ret = new double[1];
                        //    Marshal.Copy(prop.Data, ret, 0, 1);
                        //    await Dispatcher.UIThread.InvokeAsync(() =>
                        //    {
                        //    });
                        //}
                        //else if (name == "duration")
                        //{

                        //    if (prop.Data == nint.Zero)
                        //    {
                        //        Console.WriteLine("continuw");
                        //        continue;
                        //    }

                        //    var ret = new double[1];
                        //    Marshal.Copy(prop.Data, ret, 0, 1);
                        //    await Dispatcher.UIThread.InvokeAsync(() =>
                        //    {
                        //    });
                        //}
                        //else if (name == "ao-volume")
                        //{
                        //    var ret = new double[1];
                        //    Marshal.Copy(prop.Data, ret, 0, 1);
                        //    await Dispatcher.UIThread.InvokeAsync(() =>
                        //    {
                        //    });
                        //}
                        break;
                    }
                    case EventId.EndFile:
                    {
                        
                        break;
                    }
                    case EventId.None:
                    case EventId.Shutdown:
                    case EventId.LogMessage:
                    case EventId.PropertyReply:
                    case EventId.SetPropertyReply:
                    case EventId.CommandReply:
                    case EventId.StartFile:
                    case EventId.FileLoaded:
                    case EventId.Idle:
                    case EventId.Tick:
                    case EventId.Message:
                    case EventId.VideoReconfig:
                    case EventId.AudioReconfig:
                    case EventId.Seek:
                    case EventId.PlaybackRestart:
                    case EventId.QueueOverflow:
                    case EventId.Hook:
                    default:
                        break;
                }
            }
        });
    }


    public void Initialize()
    {
        int code = MpvFunctions.Initialize(_mpvHandle);
        if (code != 0)
        {
            throw new MpvException(code);
        }

    }

    public void SetOptionString(string name, string data)
    {
        nint ptrName = Marshal.StringToHGlobalAnsi(name);
        nint ptrData = Marshal.StringToHGlobalAnsi(data);

        int code = MpvFunctions.SetOptionString(_mpvHandle, ptrName, ptrData);
        
        Marshal.FreeHGlobal(ptrName);
        Marshal.FreeHGlobal(ptrData);
        
        if (code != 0)
        {
            throw new MpvException(code);
        }
        
    }

    public void SetWakeupCallback(WakeupCallback cb, nint data)
    {
        _mpvWakeupCallback = cb;
        MpvFunctions.SetWakeupCallback(_mpvHandle, _mpvWakeupCallback, data);
    }

    public void CommandNode(List<string> cmd)
    {
        nint[] argPtr = new nint[cmd.Count + 1];

        int i = 0;
        foreach (var item in cmd)
        {
            #region In order to support Chinese file name

            var bytes = Encoding.UTF8.GetBytes(item + '\0');
            nint ptr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);

            #endregion

            argPtr[i] = ptr;
            i++;
        }

        int code = MpvFunctions.Command(_mpvHandle, Marshal.UnsafeAddrOfPinnedArrayElement(argPtr, 0));

        foreach (var item in argPtr)
        {
            Marshal.FreeHGlobal(item);
        }
        if (code != 0)
        {
            throw new MpvException(code);
        }




        //int count = cmd.Count + 1;
        //IntPtr[] pointers = new IntPtr[count];
        //IntPtr rootPtr = Marshal.AllocHGlobal(IntPtr.Size * count);

        //for (int index = 0; index < cmd.Count; index++)
        //{
        //    var bytes = Encoding.UTF8.GetBytes(cmd[index] + "\0");
        //    IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        //    Marshal.Copy(bytes, 0, ptr, bytes.Length);
        //    pointers[index] = ptr;
        //}

        //Marshal.Copy(pointers, 0, rootPtr, count);
        //MpvFunctions.Command(_mpvHandle, rootPtr);
        //var node = new MpvNode
        //{
        //    Format = MpvFormat.MpvFormatNodeArray
        //};


        //var list = new MpvNodeList
        //{
        //    Num = cmd.Count
        //};


        //var toBeFree = new List<nint>();
        //var nodeList = new MpvNode[list.Num];
        //for (int i = 0; i < nodeList.Length; i++)
        //{
        //    nodeList[i].Format = MpvFormat.MpvFormatString;
        //    var ptr = Marshal.StringToHGlobalAnsi(cmd[i]);
        //    nodeList[i].Data.CString = ptr;
        //    toBeFree.Add(ptr);
        //}
        //list.NodeValues = Marshal.UnsafeAddrOfPinnedArrayElement(nodeList, 0);

        //nint listPtr = Marshal.AllocHGlobal(Marshal.SizeOf(list));
        //Marshal.StructureToPtr(list, listPtr, true);
        //node.Data.NodeList = listPtr;
        //toBeFree.Add(listPtr);

        //var nodePtr = Marshal.AllocHGlobal(Marshal.SizeOf(node));
        //Marshal.StructureToPtr(node, nodePtr, true);
        //toBeFree.Add(nodePtr);

        //MpvNode res = new MpvNode();
        //var resPtr = Marshal.AllocHGlobal(Marshal.SizeOf(res));
        //Marshal.StructureToPtr(res, resPtr, true);
        //toBeFree.Add(resPtr);

        //int code = MpvFunctions.CommandNode(_mpvHandle, nodePtr, resPtr);

        //Free(toBeFree);

        //if (code != 0)
        //{
        //    throw new MpvException(code);
        //}
    }


    public object? CommandNode(List<object> cmd)
    {
        MpvNode node = MpvNode.FromObject(cmd);
        nint nodePtr = Marshal.AllocHGlobal(Marshal.SizeOf<MpvNode>());
        Marshal.StructureToPtr(node, nodePtr, true);

        var outPtr = Marshal.AllocHGlobal(Marshal.SizeOf<MpvNode>());
        try
        {

            int code = MpvFunctions.CommandNode(_mpvHandle, nodePtr, outPtr);
            var outNode = Marshal.PtrToStructure<MpvNode>(outPtr);

            node.Free();

            object? ret = outNode.ToObject();
            if (code != 0)
            {
                throw new MpvException(code);
            }
            MpvFunctions.FreeNodeContents(outPtr);
            return ret;
        }
        finally
        {
            
            Marshal.FreeHGlobal(nodePtr);
            Marshal.FreeHGlobal(outPtr);
        }

    }

    public object? CommandNode(params object[] cmd)
    {
        return CommandNode(cmd.ToList());
    }
    public MpvEvent WaitEvent(double timeout)
    {
        var ptr = MpvFunctions.WaitEvent(_mpvHandle, timeout);
        return Marshal.PtrToStructure<MpvEvent>(ptr);
    }

    public void RenderContextCreate(Dictionary<RenderParamType, object> parameters)
    {
        
        List<nint> toBeFree = new List<nint>();
        var paraArray = new RenderParam[parameters.Count + 1];

        int i = 0;

        foreach (var item in parameters)
        {
            // nint data = item.Key switch
            // {
            //     MpvRenderParamType.MpvRenderParamInvalid => nint.Zero,
            //     
            //     _ => throw new NotImplementedException("current api is not support try to use FFI.MpvFunction")
            // };
            nint data;
            switch (item.Key)
            {
                case RenderParamType.ApiType:
                    data = Marshal.StringToHGlobalAnsi((string)item.Value);
                    toBeFree.Add(data);
                    break;
                case RenderParamType.Invalid:
                    data = nint.Zero;
                    break;
                case RenderParamType.FlipY:
                    data = Marshal.AllocHGlobal(sizeof(int));
                    Marshal.WriteInt32(data, 1);
                    toBeFree.Add(data);
                    break;
                case RenderParamType.OpenglInitParams:
                    var openglPara = (OpenglInitParams)item.Value;
                    _mpvGetProcAddressCallback = openglPara.MpvGetProcAddress;
                    openglPara.MpvGetProcAddress = _mpvGetProcAddressCallback;
                    data = Marshal.AllocHGlobal(Marshal.SizeOf<OpenglInitParams>());
                    Marshal.StructureToPtr(openglPara, data, true);
                    toBeFree.Add(data);
                    break;
                case RenderParamType.OpenglFbo:
                case RenderParamType.Depth:
                case RenderParamType.IccProfile:
                case RenderParamType.AmbientLight:
                case RenderParamType.X11Display:
                case RenderParamType.WlDisplay:
                case RenderParamType.AdvancedControl:
                case RenderParamType.NextFrameInfo:
                case RenderParamType.BlockForTargetTime:
                case RenderParamType.SkipRendering:
                case RenderParamType.DrmDisplay:
                case RenderParamType.DrmDrawSurfaceSize:
                case RenderParamType.DrmDisplayV2:
                case RenderParamType.SwSize:
                case RenderParamType.SwFormat:
                case RenderParamType.SwStride:
                case RenderParamType.SwPointer:
                default:
                    throw new NotImplementedException("current api is not support try to use FFI.MpvFunction");
            }

            paraArray[i].Data = data;
            paraArray[i].Type = item.Key;
            i++;
        }

        paraArray[i].Data = nint.Zero;
        paraArray[i].Type = RenderParamType.Invalid;
        
        var outptr = new nint [1];
        int code = MpvFunctions.RenderContextCreate(Marshal.UnsafeAddrOfPinnedArrayElement(outptr, 0), _mpvHandle, Marshal.UnsafeAddrOfPinnedArrayElement(paraArray, 0));
        
        Free(toBeFree);
        if (code != 0)
        {
            throw new MpvException(code);
        }

        _mpvrender = outptr[0];
    }

    public void RenderContextSetUpdateCallback(RenderContextUpdateCallback callback, nint data)
    {
        _renderContextUpdateCallback = callback;
        // MpvFunctions.RenderContextSetUpdateCallback(_mpvrender, Marshal.GetFunctionPointerForDelegate(_renderContextUpdateCallback), data);
        MpvFunctions.RenderContextSetUpdateCallback(_mpvrender, _renderContextUpdateCallback, data);
    }

    public void RenderContextRender(Dictionary<RenderParamType, object> parameters)
    { 
        var toBeFree = new List<nint>
        {
            Capacity = 0
        };
        var paraArray = new RenderParam[parameters.Count + 1];

        int i = 0;

        foreach (var item in parameters)
        {
            // nint data = item.Key switch
            // {
            //     MpvRenderParamType.MpvRenderParamInvalid => nint.Zero,
            //     
            //     _ => throw new NotImplementedException("current api is not support try to use FFI.MpvFunction")
            // };
            nint data;
            switch (item.Key)
            {
                case RenderParamType.ApiType:
                    data = Marshal.StringToHGlobalAnsi((string)item.Value);
                    toBeFree.Add(data);
                    break;
                case RenderParamType.Invalid:
                    data = nint.Zero;
                    break;
                case RenderParamType.FlipY:
                    data = Marshal.AllocHGlobal(sizeof(int));
                    Marshal.WriteInt32(data, 1);
                    toBeFree.Add(data);
                    break;
                case RenderParamType.OpenglInitParams:
                    var openglPara = (OpenglInitParams)item.Value;
                    data = Marshal.AllocHGlobal(Marshal.SizeOf<OpenglInitParams>());
                    Marshal.StructureToPtr(openglPara, data, true);
                    toBeFree.Add(data);
                    break;
                case RenderParamType.OpenglFbo:
                    var openglFbo = (OpenglFbo)item.Value;
                    data = Marshal.AllocHGlobal(Marshal.SizeOf<OpenglFbo>());
                    Marshal.StructureToPtr(openglFbo, data, true);
                    toBeFree.Add(data);
                    break;
                case RenderParamType.Depth:
                case RenderParamType.IccProfile:
                case RenderParamType.AmbientLight:
                case RenderParamType.X11Display:
                case RenderParamType.WlDisplay:
                case RenderParamType.AdvancedControl:
                case RenderParamType.NextFrameInfo:
                case RenderParamType.BlockForTargetTime:
                case RenderParamType.SkipRendering:
                case RenderParamType.DrmDisplay:
                case RenderParamType.DrmDrawSurfaceSize:
                case RenderParamType.DrmDisplayV2:
                case RenderParamType.SwSize:
                case RenderParamType.SwFormat:
                case RenderParamType.SwStride:
                case RenderParamType.SwPointer:
                default:
                    throw new NotImplementedException("current api is not support try to use FFI.MpvFunction");
            }

            paraArray[i].Data = data;
            paraArray[i].Type = item.Key;
            i++;
        }

        paraArray[i].Data = nint.Zero;
        paraArray[i].Type = RenderParamType.Invalid;

        int code = MpvFunctions.RenderContextRender(_mpvrender, Marshal.UnsafeAddrOfPinnedArrayElement(paraArray, 0));
        Free(toBeFree);
        if (code != 0)
        {
            throw new MpvException(code);
        }
        
    }

    public void ObserveProperty(string name, Format format)
    {
        nint namePtr = Marshal.StringToHGlobalAnsi(name);
        int code = MpvFunctions.ObserveProperty(_mpvHandle, 0, namePtr, format);
        Marshal.FreeHGlobal(namePtr);
        if (code != 0)
        {
            throw new MpvException(code);
        }
        
    }

    public object? TryGetProperty(string name)
    {
        try
        {
            return GetProperty(name);
        }
        catch (MpvException)
        {
            return null;
        }
    }
    
    public object GetProperty(string name)
    {
        var namePtr = Marshal.StringToCoTaskMemUTF8(name);
        var nodePtr = Marshal.AllocHGlobal(Marshal.SizeOf<MpvNode>());
        int code = MpvFunctions.GetProperty(_mpvHandle, namePtr, Format.Node, nodePtr);
        Marshal.FreeHGlobal(namePtr);

        if (code != 0)
        {
            Marshal.FreeHGlobal(nodePtr);
            throw new MpvException(code)
            {
                Detail = name
            };
        }

        var ret = Marshal.PtrToStructure<MpvNode>(nodePtr).ToObject();
        MpvFunctions.FreeNodeContents(nodePtr);
        Marshal.FreeHGlobal(nodePtr);
        return ret!;
    }

    public void SetProperty(string name, object data)
    {
        MpvNode node = MpvNode.FromObject(data);
        var dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<MpvNode>());
        Marshal.StructureToPtr(node, dataPtr, true);
        var namePtr = Marshal.StringToHGlobalAnsi(name);
        int code = MpvFunctions.SetProperty(_mpvHandle, namePtr, Format.Node, dataPtr);
        node.Free();
        Marshal.FreeHGlobal(namePtr);
        if (code != 0)
        {
            throw new MpvException(code);
        }
        _valueCache[name] = data;
    }
    private static void Free(List<nint> arg)
    {
        foreach (var item in arg)
        {
            Marshal.FreeHGlobal(item);
        }
    }


}
    

    


