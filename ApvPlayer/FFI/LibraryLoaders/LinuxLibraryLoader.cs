using System;
using System.Runtime.InteropServices;
using ApvPlayer.FFI.LibraryLoaders;

namespace LibMpv.LibraryLoaders;

public class LinuxLibraryLoader : ILibraryLoader
{

    private const int RtldLazy = 0x1;
    private const int RtldNow = 0x2;
    private const int RtldLocal = 0x4;
    private const int RtldGlobal =	0x8;
    [DllImport("dl", EntryPoint = "dlopen")]
    private static extern IntPtr Dlopen( string pathname, int mode);
    
    [DllImport("dl", EntryPoint = "dlsym")]
    private static extern IntPtr Dlsym(IntPtr handle,string symbol);
    
    [DllImport("dl", EntryPoint = "dlclose")]
    private static extern int Dlclose (IntPtr handle);

    private IntPtr _libraryHandler = IntPtr.Zero;
    private bool _disposed = false;

    public void Dispose()
    {
        if (_disposed != false || _libraryHandler == IntPtr.Zero) return;
        Dlclose(_libraryHandler);
        _libraryHandler = IntPtr.Zero;
        _disposed = true;
    }

    public bool LoadDynamicLibrary(string path)
    {
        _libraryHandler = Dlopen(path, RtldLazy);
        return _libraryHandler != IntPtr.Zero;
    }

    public IntPtr GetProcAddress(string symbol)
    {
        if (_libraryHandler == IntPtr.Zero)
        {
            throw new InvalidOperationException("Dynamic library is not loaded or failed to load");
        }

        return Dlsym(_libraryHandler, symbol);
    }
}