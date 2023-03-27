using Avalonia;
using System;
using System.Runtime.InteropServices;
using ApvPlayer.FFI.LibMpv;


namespace ApvPlayer;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        MpvSetUp();
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();

    private static void MpvSetUp()
    {
        string mpvPath;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            mpvPath = "./mpv-2.dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            mpvPath = "/usr/local/Cellar/mpv/0.35.1/lib/libmpv.dylib";
        }
        else
        {
            throw new NotImplementedException();
        }

        MpvFunctions.Setup(mpvPath);       
    }
}

