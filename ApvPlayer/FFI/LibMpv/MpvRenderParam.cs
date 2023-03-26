using System.Runtime.InteropServices;

namespace ApvPlayer.FFI.LibMpv;

[StructLayout(LayoutKind.Sequential)]
public struct MpvRenderParam
{
       public MpvRenderParamType Type;
       public nint Data;
}