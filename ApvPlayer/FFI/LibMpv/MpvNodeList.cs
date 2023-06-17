﻿using System.Runtime.InteropServices;

namespace ApvPlayer.FFI.LibMpv;

[StructLayout(LayoutKind.Sequential)]
public struct NodeList
{
 /**
         * Number of entries. Negative values are not allowed.
         */
    public int Num;

        /**
         * MPV_FORMAT_NODE_ARRAY:
         *  values[N] refers to value of the Nth item
         *
         * MPV_FORMAT_NODE_MAP:
         *  values[N] refers to value of the Nth key/value pair
         *
         * If num > 0, values[0] to values[num-1] (inclusive) are valid.
         * Otherwise, this can be NULL.
         */


    public nint NodeValues;

        /**
         * MPV_FORMAT_NODE_ARRAY:
         *  unused (typically NULL), access is not allowed
         *
         * MPV_FORMAT_NODE_MAP:
         *  keys[N] refers to key of the Nth key/value pair. If num > 0, keys[0] to
         *  keys[num-1] (inclusive) are valid. Otherwise, this can be NULL.
         *  The keys are in random order. The only guarantee is that keys[N] belongs
         *  to the value values[N]. NULL keys are not allowed.
         */
    public nint Key; // char** keys;
}