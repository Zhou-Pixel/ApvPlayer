using System;

namespace ApvPlayer.FFI.LibraryLoaders;

public interface ILibraryLoader : IDisposable
{
    
    bool LoadDynamicLibrary(string path);
    nint GetProcAddress(string symbol);
}