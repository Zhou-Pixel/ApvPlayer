using System;
using ApvPlayer.EventArgs;
using ApvPlayer.FFI.LibMpv;
using ApvPlayer.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ApvPlayer.Controls;

public partial class VideoControl : UserControl
{
    public VideoControl()
    {
        InitializeComponent();
        var model = new VideoControlModel
        {
            Handle = (Mpv)this.FindResource("Mpv")!
        };
        DataContext = model;
    }


    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
    }

    private void GlControl_OnMpvPropertyChanged(object sender, MpvPropertyChangedEventArgs arg)
    {

    }

    private async void ChooseButton_OnClick(object? sender, RoutedEventArgs e)
    {

    }

    private void VideoSlider_OnTextInput(object? sender, TextInputEventArgs e)
    {
        Console.WriteLine("input");
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Console.WriteLine("press input");
    }
}