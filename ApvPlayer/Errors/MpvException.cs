using System;
using System.Runtime.InteropServices;
using ApvPlayer.FFI.LibMpv;

namespace ApvPlayer.Errors;

public class MpvException : Exception
{
    public MpvError Error => (MpvError)Code;
    public int Code { get; }

    public override string Message { get; }

    public MpvException(int code)
    {
        Code = code;
        Message = Marshal.PtrToStringAnsi(MpvFunctions.Instance.ErrorString(code)) ?? string.Empty;
    }
}