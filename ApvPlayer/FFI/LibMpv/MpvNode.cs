using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ApvPlayer.FFI.LibMpv;

[StructLayout(LayoutKind.Sequential)]
public struct MpvNode
{
    public MpvNodeData Data;

    /**
     * Type of the data stored in this struct. This value rules what members in
     * the given union can be accessed. The following formats are currently
     * defined to be allowed in mpv_node:
     *
     *  MPV_FORMAT_STRING       (u.string)
     *  MPV_FORMAT_FLAG         (u.flag)
     *  MPV_FORMAT_INT64        (u.int64)
     *  MPV_FORMAT_DOUBLE       (u.double_)
     *  MPV_FORMAT_NODE_ARRAY   (u.list)
     *  MPV_FORMAT_NODE_MAP     (u.list)
     *  MPV_FORMAT_BYTE_ARRAY   (u.ba)
     *  MPV_FORMAT_NONE         (no member)
     *
     * If you encounter a value you don't know, you must not make any
     * assumptions about the contents of union u.
     */
    public MpvFormat Format;

    public static MpvNode FromObject(object data)
    {
        MpvNode node = new MpvNode
        {
            Format = MpvFormat.MpvFormatNone
        };

        switch (data)
        {
            case double or float:
                node.Format = MpvFormat.MpvFormatDouble;
                node.Data.DoubleData = (double)data;
                break;
            case bool flag:
                node.Format = MpvFormat.MpvFormatFlag;
                node.Data.Flag = flag ? 1 : 0;
                break;
            case int or uint or ulong or long:
                node.Format = MpvFormat.MpvFormatInt64;
                node.Data.IntData = (long)data;
                break;
            case string str:
                node.Format = MpvFormat.MpvFormatString;
                // node.Data.CString = Marshal.StringToHGlobalAnsi(str);
                var bytes = Encoding.UTF8.GetBytes(str + "\0");
                nint ptr = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
                node.Data.CString = ptr;
                break;
            case List<object> list:
            {
                node.Format = MpvFormat.MpvFormatNodeArray;
                node.Data.NodeList = Marshal.AllocHGlobal(Marshal.SizeOf<MpvNodeList>());
            
            
                var nodeSize = Marshal.SizeOf<MpvNode>();
                MpvNodeList nodeList = new MpvNodeList
                {
                    NodeValues = Marshal.AllocHGlobal(nodeSize * list.Count),
                    Num = list.Count
                };

                int i = 0;
                foreach (var tmp in list.Select(MpvNode.FromObject))
                {
                    Marshal.StructureToPtr(tmp, nodeList.NodeValues + i * nodeSize, true);
                    i++;
                }
            
                Marshal.StructureToPtr(nodeList, node.Data.NodeList, true);
                break;
            }
            case Dictionary<string, object> dictionary:
            {
                node.Format = MpvFormat.MpvFormatNodeMap;
                node.Data.NodeList = Marshal.AllocHGlobal(Marshal.SizeOf<MpvNodeList>());
            
                var nodeSize = Marshal.SizeOf<MpvNode>();
                MpvNodeList nodeList = new MpvNodeList()
                {
                    NodeValues = Marshal.AllocHGlobal(nodeSize * dictionary.Count),
                    Num = dictionary.Count,
                    Key = Marshal.AllocHGlobal(Marshal.SizeOf<nint>() * dictionary.Count)
                };


                int i = 0;
                foreach (var item in dictionary)
                {
                    MpvNode tmp = MpvNode.FromObject(item.Value);
                    Marshal.StructureToPtr(tmp, nodeList.NodeValues + i * nodeSize, true);
                    i++;

                    var keyPtr = Marshal.StringToHGlobalAnsi(item.Key);
                    Marshal.WriteIntPtr(nodeList.NodeValues, i * nodeSize, keyPtr);
                }
            
                Marshal.StructureToPtr(nodeList, node.Data.NodeList, true);
                break;
            }
            default:
                throw new InvalidOperationException("not support node format");
        }

        return node;
    }

    public void Free()
    {
        switch (Format)
        {
            case MpvFormat.MpvFormatString:
                Marshal.FreeHGlobal(Data.CString);
                break;
            case MpvFormat.MpvFormatNodeMap:
            {
                MpvNodeList list = Marshal.PtrToStructure<MpvNodeList>(Data.NodeList);
                Marshal.FreeHGlobal(list.NodeValues);
                for (int i = 0; i < list.Num; i++)
                {
                    var ptr = Marshal.ReadIntPtr(list.Key + i * Marshal.SizeOf<nint>());
                    Marshal.FreeHGlobal(ptr);
                }
                Marshal.FreeHGlobal(list.Key);
                Marshal.FreeHGlobal(Data.NodeList);
                break;
            }
            case MpvFormat.MpvFormatNodeArray:
            {
                MpvNodeList list = Marshal.PtrToStructure<MpvNodeList>(Data.NodeList);
                Marshal.FreeHGlobal(list.NodeValues);
                Marshal.FreeHGlobal(Data.NodeList);
                break;
            }
        }
    }
    public object? ToObject()
    {
        object? ret;
        switch (Format)
        {
            case MpvFormat.MpvFormatDouble:
            {
                ret = Data.DoubleData;
                break;
            }
            case MpvFormat.MpvFormatString:
            {
                //ret = Marshal.PtrToStringAnsi(Data.CString);
                ret = Data.CString;
                break;
            }
            case MpvFormat.MpvFormatFlag:
            {
                ret = Data.Flag != 0;
                break;
            }
            case MpvFormat.MpvFormatInt64:
            {
                ret = Data.IntData;
                break;
            }
            case MpvFormat.MpvFormatNodeArray:
            {
                var nodeList = Marshal.PtrToStructure<MpvNodeList>(Data.NodeList);
                var tmp = new List<object>();


                for (int i = 0; i < nodeList.Num; i++)
                {
                    var size = Marshal.SizeOf<MpvNode>();
                    var node = Marshal.PtrToStructure<MpvNode>(nodeList.NodeValues + i * size).ToObject();//手动处理指针偏移
                    if (node == null)
                    {
                        continue;
                    }

                    tmp.Add(node);
                }

                ret = tmp;
                break;
            }
            case MpvFormat.MpvFormatNodeMap:
            {
                var nodeList = Marshal.PtrToStructure<MpvNodeList>(Data.NodeList);
                var tmp = new Dictionary<string, object>();
                for (int i = 0; i < nodeList.Num; i++)
                {
                    var sizeNode = Marshal.SizeOf<MpvNode>();
                    var node = Marshal.PtrToStructure<MpvNode>(nodeList.NodeValues + i * sizeNode).ToObject();//手动处理指针偏移
                    if (node == null)
                    {
                        continue;
                    }

                    var sizePtr = Marshal.SizeOf<nint>();

                    var stringPtr = Marshal.ReadIntPtr(nodeList.Key + i * sizePtr);
                    string key = Marshal.PtrToStringAnsi(stringPtr)!;
                    
                    tmp.Add(key, node);
                }

                ret = tmp;
                break;
            }
            case MpvFormat.MpvFormatNone:
            case MpvFormat.MpvFormatOsdString:
            case MpvFormat.MpvFormatNode:
            case MpvFormat.MpvFormatByteArray:
            default:
                throw new NotSupportedException("not support current format node");
                
        }

        return ret;
    }
}
