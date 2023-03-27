using System;
using System.Runtime.InteropServices;

namespace ApvPlayer.FFI.LibraryLoaders;

public class MacLibraryLoader : ILibraryLoader
{
    private const int RtldLazy = 0x1;
    private const int RtldNow = 0x2;
    private const int RtldLocal = 0x4;
    private const int RtldGlobal =	0x8;
    [DllImport("dl", EntryPoint = "dlopen")]
    private static extern IntPtr Dlopen(string pathname, int mode);
    
    [DllImport("dl", EntryPoint = "dlsym")]
    private static extern nint Dlsym(nint handle, string symbol);
    
    [DllImport("dl", EntryPoint = "dlclose")]
    private static extern int Dlclose (nint handle);

    private nint _libraryHandler = nint.Zero;
    private bool _disposed = false;

    public void Dispose()
    {
        if (_disposed != false || _libraryHandler == IntPtr.Zero) return;
        Dlclose(_libraryHandler);
        _libraryHandler = nint.Zero;
        _disposed = true;
    }

    public bool LoadDynamicLibrary(string path)
    {
        _libraryHandler = Dlopen(path, RtldLazy);
        return _libraryHandler != nint.Zero;
    }

    public nint GetProcAddress(string symbol)
    {
        if (_libraryHandler == nint.Zero)
        {
            throw new InvalidOperationException("Dynamic library is not loaded or failed to load");
        }

        return Dlsym(_libraryHandler, symbol);
    }
}