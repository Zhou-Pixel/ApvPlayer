using System;
using System.Runtime.InteropServices;
using ApvPlayer.FFI.LibraryLoaders;

namespace LibMpv.LibraryLoaders;

public class WindowsLibraryLoader : ILibraryLoader
{
    
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
    private static extern int FreeLibrary(IntPtr hModule);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lProcName);

    private IntPtr _libraryHandler = IntPtr.Zero;

    private bool _disposed = false;

    ~WindowsLibraryLoader()
    {
        Dispose();
    }
    
    public void Dispose()
    {
        if (_disposed != false || _libraryHandler == IntPtr.Zero) return;
        FreeLibrary(_libraryHandler);
        _libraryHandler = IntPtr.Zero;
        _disposed = true;
    }

    public bool LoadDynamicLibrary(string path)
    {
         _libraryHandler = LoadLibrary(path);
         return _libraryHandler != IntPtr.Zero;
    }

    public IntPtr GetProcAddress(string symbol)
    {
        if (_libraryHandler == IntPtr.Zero)
        {
            throw new InvalidOperationException("Dynamic library is not loaded or failed to load");
        }
        return GetProcAddress(_libraryHandler, symbol);
    }
}