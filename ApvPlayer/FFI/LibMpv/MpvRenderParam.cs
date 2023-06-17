using System.Runtime.InteropServices;

namespace ApvPlayer.FFI.LibMpv;

[StructLayout(LayoutKind.Sequential)]
public struct RenderParam
{
       public RenderParamType Type;
       public nint Data;
}