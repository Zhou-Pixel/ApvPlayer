using System;
using System.Runtime.InteropServices;

namespace ApvPlayer.FFI.LibMpv;

[StructLayout(LayoutKind.Sequential)]
public struct MpvEventProperty
{
    /**
         * Name of the property.
         */
    public nint Name;

    /**
         * Format of the data field in the same struct. See enum mpv_format.
         * This is always the same format as the requested format, except when
         * the property could not be retrieved (unavailable, or an error happened),
         * in which case the format is MPV_FORMAT_NONE.
         */
    public MpvFormat Format;

    /**
         * Received property value. Depends on the format. This is like the
         * pointer argument passed to mpv_get_property().
         *
         * For example, for MPV_FORMAT_STRING you get the string with:
         *
         *    char *value = *(char **)(event_property->data);
         *
         * Note that this is set to NULL if retrieving the property failed (the
         * format will be MPV_FORMAT_NONE).
         */
    public nint Data;

    public object? TakeData()
    {
         if (Format == MpvFormat.MpvFormatNone || Data == nint.Zero)
         {
              return null;
         }

         object? ret;
         switch (Format)
         {
              case MpvFormat.MpvFormatDouble:
              {
                   var value = new double[1];
                   Marshal.Copy(Data, value, 0, 1);
                   ret = value[0];
                   break;
              }
              case MpvFormat.MpvFormatString:
              {
                   ret = Marshal.PtrToStringAnsi(Data);
                   break;
              }
              case MpvFormat.MpvFormatFlag:
              {
                   var value = Marshal.ReadInt32(Data);
                   ret = value != 0;
                   break;
              }
              case MpvFormat.MpvFormatInt64:
              {
                   ret = Marshal.ReadInt64(Data);
                   break;
              }
              case MpvFormat.MpvFormatNone:
              case MpvFormat.MpvFormatOsdString:
              case MpvFormat.MpvFormatNode:
              case MpvFormat.MpvFormatNodeArray:
              case MpvFormat.MpvFormatNodeMap:
              case MpvFormat.MpvFormatByteArray:
              default:
                   throw new NotImplementedException("not support now");
         }

         return ret;
    }
}