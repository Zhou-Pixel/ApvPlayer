using System;
using ApvPlayer.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace ApvPlayer.Controls;

public partial class VideoControlBar : UserControl
{
    public static readonly StyledProperty<double> DurationProperty = AvaloniaProperty.Register<VideoControlBar, double>(
        nameof(Duration), 100);

    public static readonly StyledProperty<bool> MuteProperty = AvaloniaProperty.Register<VideoControlBar, bool>(
        nameof(Mute));
    



    public static readonly StyledProperty<double> VolumeProperty = AvaloniaProperty.Register<VideoControlBar, double>(
        nameof(Volume), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> PauseProperty = AvaloniaProperty.Register<VideoControlBar, bool>(
        nameof(Pause), true);

    public static readonly StyledProperty<bool> FullScreenProperty = AvaloniaProperty.Register<VideoControlBar, bool>(
        nameof(FullScreen));

    public static readonly StyledProperty<bool> ActiveProperty = AvaloniaProperty.Register<VideoControlBar, bool>(
        nameof(Active));


    public static readonly DirectProperty<VideoControlBar, string> ProgressTextProperty = AvaloniaProperty.RegisterDirect<VideoControlBar, string>(
        nameof(ProgressText), o => o.ProgressText, (o, v) => o.ProgressText = v);



    public static readonly DirectProperty<VideoControlBar, string> PauseIconProperty = AvaloniaProperty.RegisterDirect<VideoControlBar, string>(
        nameof(PauseIcon), o => o.PauseIcon, (o, v) => o.PauseIcon = v);


    public static readonly DirectProperty<VideoControlBar, string> FullScreenIconProperty = AvaloniaProperty.RegisterDirect<VideoControlBar, string>(
        nameof(FullScreenIcon), o => o.FullScreenIcon);



    public static readonly DirectProperty<VideoControlBar, string> VolumeIconProperty = AvaloniaProperty.RegisterDirect<VideoControlBar, string>(
        nameof(VolumeIcon), o => o.VolumeIcon, (o, v) => o.VolumeIcon = v);
    

    public static readonly StyledProperty<double> PositionProperty = AvaloniaProperty.Register<VideoControlBar, double>(
        nameof(Position), defaultBindingMode: BindingMode.TwoWay);

    public double Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }
    
    private string _volumeIcon = "fa-solid fa-volume-high";
    private string VolumeIcon
    {
        get => _volumeIcon;
        set => SetAndRaise(VolumeIconProperty, ref _volumeIcon, value);
    }
    
    private string _fullScreenIcon = "fa-solid fa-expand";
    private string FullScreenIcon
    {
        get => _fullScreenIcon;
        set => SetAndRaise(FullScreenIconProperty, ref _fullScreenIcon, value);
    }


    private string _pauseIcon = "fa-solid fa-pause";
    private string PauseIcon
    {
        get => _pauseIcon;
        set => SetAndRaise(PauseIconProperty, ref _pauseIcon, value);
    }

    
    private string _progressText = "00:00:00/00:00:00";
    private string ProgressText
    {
        get => _progressText;
        set => SetAndRaise(ProgressTextProperty, ref _progressText, value);
    }
    
    public bool Active
    {
        get => GetValue(ActiveProperty);
        set => SetValue(ActiveProperty, value);
    }
    
    public bool FullScreen
    {
        get => GetValue(FullScreenProperty);
        set => SetValue(FullScreenProperty, value);
    }
    
    public bool Pause
    {
        get => GetValue(PauseProperty);
        set => SetValue(PauseProperty, value);
    }

    public double Volume
    {
        get => GetValue(VolumeProperty);
        set => SetValue(VolumeProperty, value);
    }



    public double Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }


    public bool Mute
    {
        get => GetValue(MuteProperty);
        set => SetValue(MuteProperty, value);
    }
    
    public event Action<object?, RoutedEventArgs>? VolumeClick;

    public event Action<object?, AvaloniaPropertyChangedEventArgs>? PositionChanged;

    public event Action<object?, RoutedEventArgs>? PauseClick;

    public event Action<object?, RoutedEventArgs>? StopClick;
    
    public event Action<object?, RoutedEventArgs>? FullScreenClick;
    
    public event Action<object?, RoutedEventArgs>? SetttingsClick;
    
    public event Action<object?, RoutedEventArgs>? ScreenShotClick;
    

    static VideoControlBar()
    {
    }
    
    public VideoControlBar()
    {
        InitializeComponent();
        // DataContext = new VideoControlBarModel();
        this.WhenAnyValue(o => o.Active).Subscribe(_ =>
        {
            PauseIcon = Active ? Pause ? "fa-solid fa-play" : "fa-solid fa-pause" : "fa-solid fa-play";
            ProgressText = Active ? $"{ConvertDouble(Position)}/{ConvertDouble(Duration)}" : "00:00:00/00:00:00";
        });
        this.WhenAnyValue(o => o.Position).Subscribe(_ =>
        {
            // ProgressText = "";
            ProgressText = Active ? $"{ConvertDouble(Position)}/{ConvertDouble(Duration)}" : "00:00:00/00:00:00";
        });
        this.WhenAnyValue(o => o.Duration).Subscribe(_ =>
        {
            // ProgressText = "";
            ProgressText = Active ? $"{ConvertDouble(Position)}/{ConvertDouble(Duration)}" : "00:00:00/00:00:00";
        });
        this.WhenAnyValue(o => o.Mute).Subscribe(_ =>
        {
            // VolumeIcon = "";
            VolumeIcon = Mute ? "fa-solid fa-volume-xmark" : "fa-solid fa-volume-high";
        });
        this.WhenAnyValue(o => o.FullScreen).Subscribe(_ =>
        {
            FullScreenIcon = FullScreen ? "fa-solid fa-compress" : "fa-solid fa-expand";
        });
        this.WhenAnyValue(o => o.Pause).Subscribe(_ =>
        {
            PauseIcon = Active ? Pause ? "fa-solid fa-play" : "fa-solid fa-pause" : "fa-solid fa-play";
        });
    }

    
    private static string ConvertDouble(double value)
    {
        
        if (value < 0)
        {
            throw new InvalidOperationException();
        }

        int valueInt = (int)value;
        int min = valueInt / 60;
        return min switch
        {
            0 => $"00:00:{valueInt:d2}",
            > 0 and < 60 => $"00:{min:d2}:{(min % 60):d2}",
            _ => $"{(min / 60):d2}:{(min % 60):d2}:{(valueInt % 60):d2}"
        };
    }
    
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnVolumeClick(object? sender, PointerReleasedEventArgs e)
    {
        VolumeClick?.Invoke(this, e);
    }

    private void OnVideoSliderPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            PositionChanged?.Invoke(this, e);
        }
    }

    private void OnPauseClick(object? _, RoutedEventArgs e)
    {
        PauseClick?.Invoke(this, e);
    }

    private void OnStopClick(object? _, RoutedEventArgs e)
    {
       StopClick?.Invoke(this, e);
    }

    private void OnFullScreenClick(object? _, RoutedEventArgs e)
    {
        FullScreenClick?.Invoke(this, e);
    }

    private void OnSettingsClick(object? _, RoutedEventArgs e)
    {
        SetttingsClick?.Invoke(this, e);
    }

    private void OnScreenShotClick(object? _, RoutedEventArgs e)
    {
        ScreenShotClick?.Invoke(this, e);
    }
}