using System.Runtime.InteropServices;

namespace ApvPlayer.FFI.LibMpv;

[StructLayout(LayoutKind.Explicit)]
public struct NodeData
{
    [field: FieldOffset(0)] public nint CString;

    /** valid if format==MPV_FORMAT_STRING */
    [field: FieldOffset(0)] public int Flag;

    /** valid if format==MPV_FORMAT_FLAG   */
    [field: FieldOffset(0)] public long IntData;

    /** valid if format==MPV_FORMAT_INT64  */
    [field: FieldOffset(0)] public double DoubleData;

    /** valid if format==MPV_FORMAT_DOUBLE */
    [field: FieldOffset(0)] public nint NodeList; // valid if format==MPV_FORMAT_NODE_ARRAY or if format==MPV_FORMAT_NODE_MAP

    [field: FieldOffset(0)] public nint ByteArray; // valid if format==MPV_FORMAT_BYTE_ARRAY
}