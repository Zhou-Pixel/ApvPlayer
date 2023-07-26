using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ApvPlayer.FFI.LibMpv;

[StructLayout(LayoutKind.Sequential)]
public struct Node
{
    public NodeData Data;

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
    public Format Format;

    public static Node FromObject(object data)
    {
        Node node = new Node
        {
            Format = Format.None
        };

        switch (data)
        {
            case double or float:
                node.Format = Format.Double;
                node.Data.DoubleData = (double)data;
                break;
            case bool flag:
                node.Format = Format.Flag;
                node.Data.Flag = flag ? 1 : 0;
                break;
            case int or uint or ulong or long:
                node.Format = Format.Int64;
                node.Data.IntData = (long)data;
                break;
            case string str:
                node.Format = Format.String;
                // node.Data.CString = Marshal.StringToHGlobalAnsi(str);
                //var bytes = Encoding.UTF8.GetBytes(str + "\0");
                //nint ptr = Marshal.AllocHGlobal(bytes.Length);
                //Marshal.Copy(bytes, 0, ptr, bytes.Length);
                node.Data.CString = Marshal.StringToCoTaskMemUTF8(str);
                break;
            case List<object> list:
            {
                node.Format = Format.NodeArray;
                node.Data.NodeList = Marshal.AllocHGlobal(Marshal.SizeOf<NodeList>());
            
            
                var nodeSize = Marshal.SizeOf<Node>();
                NodeList nodeList = new NodeList
                {
                    NodeValues = Marshal.AllocHGlobal(nodeSize * list.Count),
                    Num = list.Count
                };

                int i = 0;
                foreach (var tmp in list.Select(FromObject))
                {
                    Marshal.StructureToPtr(tmp, nodeList.NodeValues + i * nodeSize, false);
                    i++;
                }
            
                Marshal.StructureToPtr(nodeList, node.Data.NodeList, false);
                break;
            }
            case Dictionary<string, object> dictionary:
            {
                node.Format = Format.NodeMap;
                node.Data.NodeList = Marshal.AllocHGlobal(Marshal.SizeOf<NodeList>());
            
                var nodeSize = Marshal.SizeOf<Node>();
                NodeList nodeList = new NodeList()
                {
                    NodeValues = Marshal.AllocHGlobal(nodeSize * dictionary.Count),
                    Num = dictionary.Count,
                    Key = Marshal.AllocHGlobal(Marshal.SizeOf<nint>() * dictionary.Count)
                };


                int i = 0;
                foreach (var item in dictionary)
                {
                    Node tmp = FromObject(item.Value);
                    Marshal.StructureToPtr(tmp, nodeList.NodeValues + i * nodeSize, false);
                    var keyPtr = Marshal.StringToHGlobalAnsi(item.Key);
                    Marshal.WriteIntPtr(nodeList.Key, i * Marshal.SizeOf<nint>(), keyPtr);
                    i++;
                }
            
                Marshal.StructureToPtr(nodeList, node.Data.NodeList, false);
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
            case Format.String:
                Marshal.FreeHGlobal(Data.CString);
                break;
            case Format.NodeMap:
            {
                var list = Marshal.PtrToStructure<NodeList>(Data.NodeList);
                Marshal.FreeHGlobal(list.NodeValues);
                var nodeSize = Marshal.SizeOf<Node>();
                for (var i = 0; i < list.Num; i++)
                {
                    var ptr = Marshal.ReadIntPtr(list.Key + i * Marshal.SizeOf<nint>());
                    var node = Marshal.PtrToStructure<Node>(list.NodeValues + i * nodeSize);
                    node.Free();
                    Marshal.FreeHGlobal(ptr);
                }

                Marshal.FreeHGlobal(list.Key);
                Marshal.FreeHGlobal(Data.NodeList);
                break;
            }
            case Format.NodeArray:
            {
                NodeList list = Marshal.PtrToStructure<NodeList>(Data.NodeList);
                var nodeSize = Marshal.SizeOf<Node>();
                for (int i = 0; i < list.Num; i++)
                {
                    Node node = Marshal.PtrToStructure<Node>(list.NodeValues + i * nodeSize);
                    node.Free();
                }
                
                Marshal.FreeHGlobal(list.NodeValues);
                Marshal.FreeHGlobal(Data.NodeList);
                break;
            }
            case Format.None:
                break;
            case Format.OsdString:
                break;
            case Format.Flag:
                break;
            case Format.Int64:
                break;
            case Format.Double:
                break;
            case Format.Node:
                break;
            case Format.ByteArray:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public object? ToObject()
    {
        object? ret;
        switch (Format)
        {
            case Format.Double:
            {
                ret = Data.DoubleData;
                break;
            }
            case Format.String:
            {
                nint ptr = Data.CString;
                //var s = Marshal.PtrToStringUTF8(ptr);
                // int count = 0;
                // while (true)
                // {
                //     byte b = Marshal.ReadByte(ptr);
                //     if (b == 0)
                //         break;
                //     ptr++;
                //     count++;
                // }
                // byte[] byteString = new byte[count];
                // Marshal.Copy(ptr, byteString, 0, count);
                // ret = Encoding.Default.GetString(byteString);
                    //ret = Data.CString;
                ret = Marshal.PtrToStringUTF8(ptr);
                break;
            }
            case Format.Flag:
            {
                ret = Data.Flag != 0;
                break;
            }
            case Format.Int64:
            {
                ret = Data.IntData;
                break;
            }
            case Format.NodeArray:
            {
                var nodeList = Marshal.PtrToStructure<NodeList>(Data.NodeList);
                var tmp = new List<object>();


                for (int i = 0; i < nodeList.Num; i++)
                {
                    var size = Marshal.SizeOf<Node>();
                    var node = Marshal.PtrToStructure<Node>(nodeList.NodeValues + i * size).ToObject();//手动处理指针偏移
                    if (node == null)
                    {
                        continue;
                    }

                    tmp.Add(node);
                }

                ret = tmp;
                break;
            }
            case Format.NodeMap:
            {
                var nodeList = Marshal.PtrToStructure<NodeList>(Data.NodeList);
                var tmp = new Dictionary<string, object>();
                for (int i = 0; i < nodeList.Num; i++)
                {
                    var sizeNode = Marshal.SizeOf<Node>();
                    var node = Marshal.PtrToStructure<Node>(nodeList.NodeValues + i * sizeNode).ToObject();//手动处理指针偏移
                    if (node == null)
                    {
                        continue;
                    }

                    var sizePtr = Marshal.SizeOf<nint>();

                    var stringPtr = Marshal.ReadIntPtr(nodeList.Key + i * sizePtr);
                    string key = Marshal.PtrToStringUTF8(stringPtr)!;
                    
                    tmp.Add(key, node);
                }

                ret = tmp;
                break;
            }
            case Format.None:
                return null;
            case Format.OsdString:
            case Format.Node:
            case Format.ByteArray:
            default:
                //throw new NotSupportedException("not support current format node");
                Console.WriteLine($"not support current format node ==> {Format}");
                return null;
                
        }
        return ret;
    }
}
