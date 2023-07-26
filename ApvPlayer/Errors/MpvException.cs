using System;
using System.Runtime.InteropServices;
using ApvPlayer.FFI.LibMpv;

namespace ApvPlayer.Errors;

public class MpvException : Exception
{
    public Error Error => (Error)Code;
    public int Code { get; }

    public object? Detail;

    public override string Message { get; }

    public MpvException(int code)
    {
        Code = code;
        Message = Marshal.PtrToStringAnsi(Functions.Instance.ErrorString(code)) ?? string.Empty;
    }
}