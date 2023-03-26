using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ApvPlayer.Attributes;
using ApvPlayer.Errors;
using ApvPlayer.EventArgs;
using Avalonia.Threading;

namespace ApvPlayer.FFI.LibMpv;

public class Mpv
{
    private static readonly MpvFunctions MpvFunctions = MpvFunctions.Instance;

    private readonly nint _mpvHandle = MpvFunctions.Create();
    private nint _mpvrender = nint.Zero;
    private MpvRenderContextUpdateCallback? _renderContextUpdateCallback;
    private MpvWakeupCallback? _mpvWakeupCallback;
    private MpvGetProcAddressCallback? _mpvGetProcAddressCallback;

    public Mpv()
    {
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

    public void SetWakeupCallback(MpvWakeupCallback cb, nint data)
    {
        _mpvWakeupCallback = cb;
        MpvFunctions.SetWakeupCallback(_mpvHandle, Marshal.GetFunctionPointerForDelegate(_mpvWakeupCallback), data);
    }

    public void CommandNode(List<string> cmd)
    {
        var node = new MpvNode
        {
            Format = MpvFormat.MpvFormatNodeArray
        };


        var list = new MpvNodeList
        {
            Num = cmd.Count
        };


        var toBeFree = new List<nint>();
        var nodeList = new MpvNode[list.Num];
        for (int i = 0; i < nodeList.Length; i++)
        {
            nodeList[i].Format = MpvFormat.MpvFormatString;
            var ptr = Marshal.StringToHGlobalAnsi(cmd[i]);
            nodeList[i].Data.CString = ptr;
            toBeFree.Add(ptr);
        }
        list.NodeValues = Marshal.UnsafeAddrOfPinnedArrayElement(nodeList, 0);

        nint listPtr = Marshal.AllocHGlobal(Marshal.SizeOf(list));
        Marshal.StructureToPtr(list, listPtr, true);
        node.Data.NodeList = listPtr;
        toBeFree.Add(listPtr);

        var nodePtr = Marshal.AllocHGlobal(Marshal.SizeOf(node));
        Marshal.StructureToPtr(node, nodePtr, true);
        toBeFree.Add(nodePtr);

        MpvNode res = new MpvNode();
        var resPtr = Marshal.AllocHGlobal(Marshal.SizeOf(res));
        Marshal.StructureToPtr(res, resPtr, true);
        toBeFree.Add(resPtr);
        
        int code = MpvFunctions.CommandNode(_mpvHandle, nodePtr, resPtr);

        Free(toBeFree);
        
        if (code != 0)
        {
            throw new MpvException(code);
        }
    }


    public void CommandNode(List<object> cmd)
    {
        MpvNode node = MpvNode.FromObject(cmd);
        nint nodePtr = Marshal.AllocHGlobal(Marshal.SizeOf<MpvNode>());
        Marshal.StructureToPtr(node, nodePtr, true);

        var outPtr = Marshal.AllocHGlobal(Marshal.SizeOf<MpvNode>());
        int code = MpvFunctions.CommandNode(_mpvHandle, nodePtr, outPtr);
        
        node.Free();
        
        Marshal.FreeHGlobal(nodePtr);
        Marshal.FreeHGlobal(outPtr);
        if (code != 0)
        {
            throw new MpvException(code);
        }
    }

    public void CommandNode(params object[] cmd)
    {
        CommandNode(cmd.ToList());
    }
    public MpvEvent WaitEvent(double timeout)
    {
        var ptr = MpvFunctions.WaitEvent(_mpvHandle, timeout);
        return Marshal.PtrToStructure<MpvEvent>(ptr);
    }

    public void RenderContextCreate(Dictionary<MpvRenderParamType, object> parameters)
    {
        
        List<nint> toBeFree = new List<nint>();
        var paraArray = new MpvRenderParam[parameters.Count + 1];

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
                case MpvRenderParamType.MpvRenderParamApiType:
                    data = Marshal.StringToHGlobalAnsi((string)item.Value);
                    toBeFree.Add(data);
                    break;
                case MpvRenderParamType.MpvRenderParamInvalid:
                    data = nint.Zero;
                    break;
                case MpvRenderParamType.MpvRenderParamFlipY:
                    data = Marshal.AllocHGlobal(sizeof(int));
                    Marshal.WriteInt32(data, 1);
                    toBeFree.Add(data);
                    break;
                case MpvRenderParamType.MpvRenderParamOpenglInitParams:
                    var openglPara = (MpvOpenglInitParams)item.Value;
                    _mpvGetProcAddressCallback = openglPara.MpvGetProcAddress;
                    openglPara.MpvGetProcAddress = _mpvGetProcAddressCallback;
                    data = Marshal.AllocHGlobal(Marshal.SizeOf<MpvOpenglInitParams>());
                    Marshal.StructureToPtr(openglPara, data, true);
                    toBeFree.Add(data);
                    break;
                case MpvRenderParamType.MpvRenderParamOpenglFbo:
                case MpvRenderParamType.MpvRenderParamDepth:
                case MpvRenderParamType.MpvRenderParamIccProfile:
                case MpvRenderParamType.MpvRenderParamAmbientLight:
                case MpvRenderParamType.MpvRenderParamX11Display:
                case MpvRenderParamType.MpvRenderParamWlDisplay:
                case MpvRenderParamType.MpvRenderParamAdvancedControl:
                case MpvRenderParamType.MpvRenderParamNextFrameInfo:
                case MpvRenderParamType.MpvRenderParamBlockForTargetTime:
                case MpvRenderParamType.MpvRenderParamSkipRendering:
                case MpvRenderParamType.MpvRenderParamDrmDisplay:
                case MpvRenderParamType.MpvRenderParamDrmDrawSurfaceSize:
                case MpvRenderParamType.MpvRenderParamDrmDisplayV2:
                case MpvRenderParamType.MpvRenderParamSwSize:
                case MpvRenderParamType.MpvRenderParamSwFormat:
                case MpvRenderParamType.MpvRenderParamSwStride:
                case MpvRenderParamType.MpvRenderParamSwPointer:
                default:
                    throw new NotImplementedException("current api is not support try to use FFI.MpvFunction");
            }

            paraArray[i].Data = data;
            paraArray[i].Type = item.Key;
            i++;
        }

        paraArray[i].Data = nint.Zero;
        paraArray[i].Type = MpvRenderParamType.MpvRenderParamInvalid;
        
        var outptr = new nint [1];
        int code = MpvFunctions.RenderContextCreate(Marshal.UnsafeAddrOfPinnedArrayElement(outptr, 0), _mpvHandle, Marshal.UnsafeAddrOfPinnedArrayElement(paraArray, 0));
        
        Free(toBeFree);
        if (code != 0)
        {
            throw new MpvException(code);
        }

        _mpvrender = outptr[0];
    }

    public void RenderContextSetUpdateCallback(MpvRenderContextUpdateCallback callback, nint data)
    {
        _renderContextUpdateCallback = callback;
        MpvFunctions.RenderContextSetUpdateCallback(_mpvrender, Marshal.GetFunctionPointerForDelegate(_renderContextUpdateCallback), data);
    }

    public void RenderContextRender(Dictionary<MpvRenderParamType, object> parameters)
    { 
        List<nint> toBeFree = new List<nint>();
        var paraArray = new MpvRenderParam[parameters.Count + 1];

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
                case MpvRenderParamType.MpvRenderParamApiType:
                    data = Marshal.StringToHGlobalAnsi((string)item.Value);
                    toBeFree.Add(data);
                    break;
                case MpvRenderParamType.MpvRenderParamInvalid:
                    data = nint.Zero;
                    break;
                case MpvRenderParamType.MpvRenderParamFlipY:
                    data = Marshal.AllocHGlobal(sizeof(int));
                    Marshal.WriteInt32(data, 1);
                    toBeFree.Add(data);
                    break;
                case MpvRenderParamType.MpvRenderParamOpenglInitParams:
                    var openglPara = (MpvOpenglInitParams)item.Value;
                    data = Marshal.AllocHGlobal(Marshal.SizeOf<MpvOpenglInitParams>());
                    Marshal.StructureToPtr(openglPara, data, true);
                    toBeFree.Add(data);
                    break;
                case MpvRenderParamType.MpvRenderParamOpenglFbo:
                    var openglFbo = (MpvOpenglFbo)item.Value;
                    data = Marshal.AllocHGlobal(Marshal.SizeOf<MpvOpenglFbo>());
                    Marshal.StructureToPtr(openglFbo, data, true);
                    toBeFree.Add(data);
                    break;
                case MpvRenderParamType.MpvRenderParamDepth:
                case MpvRenderParamType.MpvRenderParamIccProfile:
                case MpvRenderParamType.MpvRenderParamAmbientLight:
                case MpvRenderParamType.MpvRenderParamX11Display:
                case MpvRenderParamType.MpvRenderParamWlDisplay:
                case MpvRenderParamType.MpvRenderParamAdvancedControl:
                case MpvRenderParamType.MpvRenderParamNextFrameInfo:
                case MpvRenderParamType.MpvRenderParamBlockForTargetTime:
                case MpvRenderParamType.MpvRenderParamSkipRendering:
                case MpvRenderParamType.MpvRenderParamDrmDisplay:
                case MpvRenderParamType.MpvRenderParamDrmDrawSurfaceSize:
                case MpvRenderParamType.MpvRenderParamDrmDisplayV2:
                case MpvRenderParamType.MpvRenderParamSwSize:
                case MpvRenderParamType.MpvRenderParamSwFormat:
                case MpvRenderParamType.MpvRenderParamSwStride:
                case MpvRenderParamType.MpvRenderParamSwPointer:
                default:
                    throw new NotImplementedException("current api is not support try to use FFI.MpvFunction");
            }

            paraArray[i].Data = data;
            paraArray[i].Type = item.Key;
            i++;
        }

        paraArray[i].Data = nint.Zero;
        paraArray[i].Type = MpvRenderParamType.MpvRenderParamInvalid;

        int code = MpvFunctions.RenderContextRender(_mpvrender, Marshal.UnsafeAddrOfPinnedArrayElement(paraArray, 0));
        Free(toBeFree);
        if (code != 0)
        {
            throw new MpvException(code);
        }
        
    }

    public void ObserveProperty(string name, MpvFormat format)
    {
        nint namePtr = Marshal.StringToHGlobalAnsi(name);
        int code = MpvFunctions.ObserveProperty(_mpvHandle, 0, namePtr, format);
        Marshal.FreeHGlobal(namePtr);
        if (code != 0)
        {
            throw new MpvException(code);
        }
        
    }

    public object GetProperty(string name)
    {
        var namePtr = Marshal.StringToHGlobalAnsi(name);
        var nodePtr = Marshal.AllocHGlobal(Marshal.SizeOf<MpvNode>());
        int code = MpvFunctions.GetProperty(_mpvHandle, namePtr, MpvFormat.MpvFormatNode, nodePtr);
        Marshal.FreeHGlobal(namePtr);

        if (code != 0)
        {
            Marshal.FreeHGlobal(nodePtr);
            throw new MpvException(code);
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
        int code = MpvFunctions.SetProperty(_mpvHandle, namePtr, MpvFormat.MpvFormatNode, dataPtr);
        node.Free();
        Marshal.FreeHGlobal(namePtr);
        if (code != 0)
        {
            throw new MpvException(code);
        }
    }
    private static void Free(List<nint> arg)
    {
        foreach (var item in arg)
        {
            Marshal.FreeHGlobal(item);
        }
    }
}
    

    


