using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApvPlayer.EventArgs;
using ApvPlayer.FFI.LibMpv;
using ApvPlayer.Views;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;
using ReactiveUI;

namespace ApvPlayer.ViewModels;

public class VideoControlModel : ViewModelBase
{

    // private Mpv? _handle;
    //
    // public Mpv Handle
    // {
    //     set
    //     {
    //         value.MpvPropertyChanged += MpvPropertyChanged;
    //         _handle = value;
    //         _handle.ObserveProperty("idle-active", MpvFormat.MpvFormatFlag);
    //     }
    // }


    public event Action<object>? RequestUpdateGl;
    public event Action<object, bool>? RequestFullScreen;

    public event Func<object, FilePickerOpenOptions, Task<IReadOnlyList<IStorageFile>>>? RequestOpenFile;

    // public required VideoControl Owner { private get; init; }


    public Mpv Handle { get; } = new();

    public VideoControlModel()
    {
        _active = !(bool)Handle.GetProperty("idle-active");
        Handle.ObserveProperty("idle-active", MpvFormat.MpvFormatFlag);
        Handle.ObserveProperty("seekable", MpvFormat.MpvFormatFlag);
        Handle.ObserveProperty("pause", MpvFormat.MpvFormatFlag);
        Handle.MpvPropertyChanged += MpvPropertyChanged;
        this.WhenAnyValue(o => o.FullScreen)
            .Subscribe(o =>
            {
                this.RaisePropertyChanged(nameof(InnerControlBarOpacity));
                this.RaisePropertyChanged(nameof(FullScreenText));
            });
        this.WhenAnyValue(o => o.IsOver)
            .Subscribe(o => this.RaisePropertyChanged(nameof(InnerControlBarOpacity)));
        this.WhenAnyValue(o => o.VideoDuration)
            .Subscribe(o => this.RaisePropertyChanged(nameof(ProgressText)));
        this.WhenAnyValue(o => o.VideoValue)
            .Subscribe(o => this.RaisePropertyChanged(nameof(ProgressText)));
    }


    private double _videoDuration = 100;

    public double VideoDuration
    {
        set => this.RaiseAndSetIfChanged(ref _videoDuration, value);
        get => _videoDuration;
    }

    private bool _seekable = true;

    public double VideoValue
    {
        set
        {
            if (!_seekable) return;
            _videoValue = value;
            if (Active)
            {
                Handle.SetProperty("time-pos", value);
            }

        }
        get => _videoValue;
    }

    private double _videoValue;

    public double VolumeValue
    {
        set => Handle.SetProperty("ao-volume", value);
        get => _active ? (double)Handle.GetProperty("ao-volume") : 50;
        //try
        //{
        //    var value = (double)Handle.GetProperty("ao-volume");
        //    return value;
        //}
        //catch (MpvException e)
        //{
        //    Console.WriteLine($"get err {e.Code}");
        //    return 0;
        //}
    }

    public bool Pause
    {
        set => Handle.SetProperty("pause", value);
        get => (bool)Handle.GetProperty("pause");
    }


    private bool _active;

    public bool Active
    {
        set => this.RaiseAndSetIfChanged(ref _active, value);
        get => _active;
    }

    private string _pauseIcon = "fa-solid fa-play";
    
    public string PauseIcon
    {
        set => this.RaiseAndSetIfChanged(ref _pauseIcon, value);
        get => _pauseIcon;
    }

    private bool _fullScreen;
    public bool FullScreen
    {
        private set
        {
            this.RaiseAndSetIfChanged(ref _fullScreen, value);
            RequestFullScreen?.Invoke(this, value);
        }
        get => _fullScreen;
    }

    public string FullScreenText => FullScreen ? "fa-solid fa-compress" : "fa-solid fa-expand";

    public double InnerControlBarOpacity
    {
        get
        {

            return IsOver && FullScreen ? 1 : 0;
        }
        set
        {
            
        }
    }

    private bool _isOver = false;

    public bool IsOver
    {
        set => this.RaiseAndSetIfChanged(ref _isOver, value);
        get => _isOver;
    }

    public async Task SwitchState()
    {
        //await DialogHostAvalonia.DialogHost.Show(new Button()
        //{
        //    Content = "click",
        //});
        // var control = (VideoControl)para;

        if (Active)
        {
            Pause = !Pause;
        }
        else
        {
            await OpenLocalFileVideo();
        }


        //_handle?.CommandNode("loadfile", st);
        //gl?.OpenFile(st);
    }

    public async Task OpenLocalFileVideo()
    {
        if (RequestOpenFile == null)
            return;
        
        var openOptions = new FilePickerOpenOptions()
        {
            Title = "选择视频",
            FileTypeFilter = new[]
            {
                new FilePickerFileType("视频")
                {
                    Patterns = new[] { "*.mp4", "*.mkv", "*.flv" }
                }
            },
            AllowMultiple = false

        };

        var ret = await RequestOpenFile.Invoke(this, openOptions);
        
        // var window = TopLevel.GetTopLevel(Owner);
        // if (window == null) return;
        // var ret = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        // {
        //     Title = "选择视频",
        //     FileTypeFilter = new[]
        //     {
        //         new FilePickerFileType("视频")
        //         {
        //             Patterns = new []{"*.mp4", "*.mkv", "*.flv"}
        //         }
        //     },
        //     AllowMultiple = false
        //
        // });
        if (ret.Count == 0) return;

        var file = Uri.UnescapeDataString(ret[0].Path.AbsolutePath);

        Handle.CommandNode(new List<string>()
        {
            "loadfile",
            file
        });
    }

    public string ProgressText =>
        Active ? $"{ConvertDouble(VideoValue)}/{ConvertDouble(VideoDuration)}" : "00:00:00/00:00:00";

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
            > 0 and < 60 => $"00:{min:d3}:{(min % 60):d2}",
            _ => $"{(min / 60):d2}:{(min % 60):d2}:{(valueInt % 60):d2}"
        };
    }

    public async Task ShowAboutDialog()
    {
        await DialogHost.Show(new AboutDialog(), "Dialog");
    }


    public void ExitFullScreen()
    {
        if (FullScreen)
            FullScreen = false;
    }
    
    public void SwitchFullScreen()
    {
        FullScreen = !FullScreen;
    }

    private void MpvPropertyChanged(object sender, MpvPropertyChangedEventArgs arg)
    {
        switch (arg.MpvPropertyName)
        {
            case "duration":
                VideoDuration = (double)arg.NewValue;
                Console.WriteLine($"duration ==> {_videoDuration}");
                break;
            case "time-pos":
            {
                if (!arg.FromMpv) break;
                var value = (double)arg.NewValue;
                if (Math.Abs(_videoValue - value) > 0.0001)
                {
                    _videoValue = value;
                    this.RaisePropertyChanged(nameof(VideoValue));
                }
                break;
            }

            case "idle-active":
            {
                Active = !(bool)arg.NewValue;
                if (!Active)
                {
                    RequestUpdateGl?.Invoke(this);
                    _videoValue = 0;
                    this.RaisePropertyChanged(nameof(VideoValue));
                }
                //this.RaisePropertyChanged(nameof(Active));
                PauseIcon = Active ? "fa-solid fa-pause" : "fa-solid fa-play";
                break;

            }
            case "seekable":
            {
                _seekable = (bool)arg.NewValue;
                break;
            }
            case "pause":
            {
                var value = (bool)arg.NewValue;
                PauseIcon = Active ? value ? "fa-solid fa-play" : "fa-solid fa-pause" : "fa-solid fa-play";
                //Icon = value ? "fa-solid fa-play" : "fa-solid fa-pause";
                break;
            }

        }
    }

    public void ActiveProperty(string name)
    {
        var info = GetType().GetProperty(name) ?? throw new ArgumentException("Property not found");
        MpvFormat format;
        var type = info.PropertyType;
        if (type == typeof(double))
        {
            format = MpvFormat.MpvFormatDouble;
        }
        else if (type == typeof(bool))
        {
            format = MpvFormat.MpvFormatFlag;
        }
        else if (type == typeof(string))
        {
            format = MpvFormat.MpvFormatString;
        }
        else if (type == typeof(int) || type == typeof(uint) || type == typeof(ulong) || type == typeof(long))
        {
            format = MpvFormat.MpvFormatInt64;
        }
        else
        {
            throw new NotImplementedException();
        }
        Handle.ObserveProperty(name, format);
    }

    public void Stop()
    {
        Handle.CommandNode("stop", "keep-playlist");
    }

    //private void RaiseMpvPropertyChanged(string mpvPropertyName)
    //{
    //    foreach (var info in this.GetType().GetProperties())
    //    {
    //        foreach (var attribute in info.GetCustomAttributes())
    //        {
    //            if (attribute is not MpvPropertyAttribute mpvAttribute) continue;
    //            if (mpvAttribute.MpvPropertyName != mpvPropertyName) continue;
                
    //            return;
    //        }
    //    }

    //    throw new Exception($"mpv property not found: {mpvPropertyName}");
    //}

    //private bool SetMpvField(string name, object value)
    //{
    //    var field = this.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
    //    if (field == null)
    //    {
    //        throw new Exception("field not found");
    //    }

    //    var orignValue = field.GetValue(this);
    //    if (value.Equals(orignValue))
    //    {
    //        return false;
    //    }
    //    field.SetValue(this, value);
    //    return true;
    //}
    
}