﻿using Avalonia;
using System;
using System.Runtime.InteropServices;
using ApvPlayer.FFI.LibMpv;
using Avalonia.ReactiveUI;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;


namespace ApvPlayer;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            MpvSetUp();
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Console.WriteLine($"catch error {e.Message}");
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current.Register<FontAwesomeIconProvider>();
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }

    private static void MpvSetUp()
    {
        string mpvPath;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            mpvPath = "./mpv-2.dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            mpvPath = "/usr/local/Cellar/mpv/0.35.1_1/lib/libmpv.dylib";
        }
        else
        {
            throw new NotImplementedException();
        }

        Functions.Setup(mpvPath);       
    }
}

