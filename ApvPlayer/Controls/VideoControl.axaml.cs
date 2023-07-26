using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApvPlayer.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

namespace ApvPlayer.Controls;

public partial class VideoControl : UserControl
{

    public static readonly StyledProperty<string?> FileNameProperty = AvaloniaProperty.Register<VideoControl, string?>(
        "FileName");

    public string? FileName
    {
        get => GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, value);
    }

    private WindowState _windowState = WindowState.Normal;

    public VideoControl()
    {
        InitializeComponent();
        var model = new VideoControlModel();
        model.RequestUpdateGl += UpdateGl;
        model.RequestFullScreen += SwitchFullScreen;
        model.RequestOpenFile += GetOpenFile;
        DataContext = model;
        Unloaded += async (sender, args) =>
        {
            await model.Unloaded();
        };
    }


    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void UpdateGl(object sender)
    {
        this.FindControl<OpenGlControl>("MpvControl")?.RequestNextFrameRendering();
    }

    private Task<IReadOnlyList<IStorageFile>> GetOpenFile(object sender, FilePickerOpenOptions opt)
    {
        return TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(opt);
    }

    private void SwitchFullScreen(object sender, bool full)
    {
        if (TopLevel.GetTopLevel(this) is not Window window)
            return;
        if (window.WindowState != WindowState.FullScreen && full)
        {
            _windowState = window.WindowState;
            window.WindowState = WindowState.FullScreen;
        }

        if (window.WindowState == WindowState.FullScreen && !full)
        {
            window.WindowState = _windowState;
        }
    }

}