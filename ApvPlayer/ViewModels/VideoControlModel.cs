using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ApvPlayer.Errors;
using ApvPlayer.EventArgs;
using ApvPlayer.FFI.LibMpv;
using ApvPlayer.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;
using ReactiveUI;
using CheckBox = ApvPlayer.Controls.CheckBox;

namespace ApvPlayer.ViewModels;

public class VideoControlModel : ViewModelBase
{


    public event Action<object>? RequestUpdateGl;
    public event Action<object, bool>? RequestFullScreen;

    public event Func<object, FilePickerOpenOptions, Task<IReadOnlyList<IStorageFile>>>? RequestOpenFile;


    private bool _playbackInitialized = false;

    public Mpv Handle { get; } = new();

    public VideoControlModel()
    {
        _active = !(bool)Handle.GetProperty("idle-active");
        Handle.ObserveProperty("idle-active", MpvFormat.MpvFormatFlag);
        Handle.ObserveProperty("seekable", MpvFormat.MpvFormatFlag);
        Handle.ObserveProperty("pause", MpvFormat.MpvFormatFlag);
        Handle.MpvPropertyChanged += OnMpvPropertyChanged;
        Handle.MpvEventReceived += OnMpvEventReceived;
        
        this.WhenAnyValue(o => o.FullScreen)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(ControlBarEnable));
                this.RaisePropertyChanged(nameof(InnerControlBarOpacity));
                this.RaisePropertyChanged(nameof(FullScreenText));
            });

        this.WhenAnyValue(o => o.IsPointerOverInnerBar)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(ControlBarEnable));
                this.RaisePropertyChanged(nameof(InnerControlBarOpacity));
            });

        this.WhenAnyValue(o => o.VideoDuration)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(ProgressText)));

        this.WhenAnyValue(o => o.VideoValue)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(ProgressText)));

        this.WhenAnyValue(o => o.SubtitleTracks)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(SubtitleMenuItems)));

        AddSubtitleCommand = ReactiveCommand.Create(AddSubTitle);
        SelectSubtitleCommand = ReactiveCommand.Create<MenuItem>(SelectSubtitle);
    }

    private void OnMpvEventReceived(object sender, MpvEventReceivedArgs args)
    {
        if (args.Evnet.EventId == MpvEventId.MpvEventPlaybackRestart && !_playbackInitialized)
        {
            _playbackInitialized = true;
            Handle.SetProperty("ao-volume", _volumeValue);
            Handle.SetProperty("ao-mute", _isMute);
            UpdateTracks();
        }

    }

    public ReactiveCommand<Unit, Task> AddSubtitleCommand { get; init; }

    public ReactiveCommand<MenuItem, Unit> SelectSubtitleCommand { get; init; }

    #region Property

    

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

    private double _volumeValue = 50;
    
    public double VolumeValue
    {
        set
        {
            if (Active)
                Handle.SetProperty("ao-volume", value);

            this.RaiseAndSetIfChanged(ref _volumeValue, value);
        }
        get => Active ? (double)Handle.GetProperty("ao-volume") : _volumeValue;
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

    public double InnerControlBarOpacity => IsPointerOverInnerBar && FullScreen ? 1 : 0;

    public bool ControlBarEnable => IsPointerOverInnerBar && FullScreen;
    // public double InnerControlBarOpacity => 1;

    private bool _isPointerOverInnerBar;

    public bool IsPointerOverInnerBar
    {
        set => this.RaiseAndSetIfChanged(ref _isPointerOverInnerBar, value);
        get => _isPointerOverInnerBar;
    }

    private bool _isMute = false;

    public bool IsMute
    {
        get => Active ? (bool)Handle.GetProperty("ao-mute") : _isMute;
        set
        {
            if (Active)
                Handle.SetProperty("ao-mute", value);
            VolumeIcon = value ? "fa-solid fa-volume-xmark" : "fa-solid fa-volume-high";
            this.RaiseAndSetIfChanged(ref _isMute, value);
        }
    }

    private string _volumeIcon = "fa-solid fa-volume-high";
    public string VolumeIcon
    {
        set => this.RaiseAndSetIfChanged(ref _volumeIcon, value);
        get => _volumeIcon;
    }


    // private Dictionary<long, string?>? _videoTrack;
    //
    // public Dictionary<long, string?>? VideoTrack 
    // {
    //     set => this.RaiseAndSetIfChanged(ref _videoTrack, value);
    //     get => _videoTrack; 
    // }
    //
    // private Dictionary<long, string?>? _subtitleTrack;
    //
    // public Dictionary<long, string?>? SubtitleTrack
    // {
    //     set => this.RaiseAndSetIfChanged(ref _subtitleTrack, value);
    //     get => _subtitleTrack;
    // }
    //
    // private Dictionary<long, string?>? _audioTrack;
    //
    // public Dictionary<long, string?>? AudioTrack
    // {
    //     set => this.RaiseAndSetIfChanged(ref _audioTrack, value);
    //     get => _audioTrack;
    // }


    private HashSet<Track> _subtitleTracks = new();
    public HashSet<Track> SubtitleTracks
    {
        get => _subtitleTracks;
        set => this.RaiseAndSetIfChanged(ref _subtitleTracks, value);
    }

    
    private HashSet<Track> _audioTracks = new();
    public HashSet<Track> AudioTracks { get => _audioTracks; set => this.RaiseAndSetIfChanged(ref _audioTracks, value); }

    
    private HashSet<Track> _videoTracks = new();
    public HashSet<Track> VideoTracks { get => _videoTracks; set => this.RaiseAndSetIfChanged(ref _videoTracks, value); }
    
    public List<MenuItem> SubtitleMenuItems
    {
        get
        {
            var list = new List<MenuItem>();
            var sub = TryGetCurrentSubtitleTrack();
            var addBinding = new Binding()
            {
                Source = this,
                Path = nameof(AddSubtitleCommand),
                Mode = BindingMode.OneWay
            };
            MenuItem addItem = new MenuItem
            {
                Header = "添加字幕",
            };
            addItem.Bind(MenuItem.CommandProperty, addBinding);

            list.Add(addItem);

            if (SubtitleTracks.Count == 0) return list;

            foreach (var i in SubtitleTracks)
            {
                var item = new MenuItem()
                {
                    Header = i.Title ?? string.Empty,
                    Icon = new ApvPlayer.Controls.CheckBox()
                    {
                        CheckedIcon = "fa-solid fa-check",
                        UnCheckedIcon = "",
                        IsChecked = sub == i.Id
                    },
                };
                var subBinding = new Binding()
                {
                    Source = this,
                    Path = nameof(SelectSubtitleCommand),
                    Mode = BindingMode.OneWay
                };
                item.Bind(MenuItem.CommandProperty, subBinding);
                var paraBinding = new Binding()
                {
                    Source = item,
                    Path = "$self",
                };
                item.Bind(MenuItem.CommandParameterProperty, paraBinding);
                list.Add(item);
                i.Item = item;
            }

            return list;

        }
    }

    public string VideoFormat => (string)Handle.GetProperty("video-format");
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


    public void SwitchMute()
    {
        IsMute = !IsMute;
    }
    #endregion

    #region Commands 

    

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

        var retNode = Handle.CommandNode("loadfile", file);
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
            > 0 and < 60 => $"00:{min:d2}:{(min % 60):d2}",
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
    
    

    private void OnMpvPropertyChanged(object sender, MpvPropertyChangedEventArgs arg)
    {
        switch (arg.MpvPropertyName)
        {
            case "duration":
                VideoDuration = (double)arg.NewValue;
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
                    _playbackInitialized = false;
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


    public void SelectSubtitle( MenuItem menu)
    {
        try
        {

            foreach (var i in SubtitleTracks)
            {
                // if (i.Item == null) continue;

                bool check = false;
                if (Equals(i.Item, menu))
                {
                    Handle.SetProperty("sid", i.Id);
                    check = true;
                    // if (i.Item.Icon is not CheckBox checkBox) continue;
                }

                if (i.Item?.Icon is CheckBox checkBox)
                {
                    checkBox.IsChecked = check;
                }
            }
            
            // Handle.SetProperty("sid", menu.Item1);
            // long id = (long)Handle.GetProperty("sid");
            // if (menu.Item2.Icon is not CheckBox checkbox) return;
            // if (checkbox.IsChecked)
            // {
            //     Handle.CommandNode("sub-remove");
            // }
            // else
            // {
            //     SubtitleMenuItems.ForEach(i =>
            //     {
            //                        
            //     });
            // }
        }
        catch (MpvException e)
        {
            Console.WriteLine($"detail {e.Detail} {e.Message}");
        }
    }

    public async Task AddSubTitle()
    {
        if (!Active) return; 
        if (RequestOpenFile == null) return;
        var openOptions = new FilePickerOpenOptions()
        {
            Title = "选择字幕",
            FileTypeFilter = new[]
            {
                new FilePickerFileType("字幕")
                {
                    Patterns = new[] { "*.ass", "*.sup", "*.flv" }
                }
            },
            AllowMultiple = false

        };
        var ret = await RequestOpenFile.Invoke(this, openOptions);
        if (ret.Count == 0) return;

        var file = ret[0].Path.LocalPath;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            file = @"\\?\" + file;
        }

        var node = Handle.CommandNode("sub-add", file);
        if (node != null)
        {
            var no = MpvNode.FromObject(node);
            Console.WriteLine($"format add sub ==> {no.Format}");
        }
        else
        {
            Console.WriteLine("sub add no ret");
        }
        UpdateTracks();
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
    #endregion

    private void UpdateTracks()
    {
        var videoTrack =    new HashSet<Track>();
        var subtitleTrack = new HashSet<Track>();
        var audioTrack =    new HashSet<Track>();
        long count = (long)Handle.GetProperty("track-list/count");
        for (int i = 0; i < count; i++)
        {
            try
            {

                var track = new Track()
                {
                    Id = (long)Handle.GetProperty($"track-list/{i}/id"),
                    Type = (string)Handle.GetProperty($"track-list/{i}/type"),
                    Title = (string?)Handle.TryGetProperty($"track-list/{i}/title")
                };
                switch (track.Type)
                {
                    case "video":
                        videoTrack.Add(track);
                        break;
                    case "audio":
                        audioTrack.Add(track);
                        break;
                    case "sub":
                        subtitleTrack.Add(track);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (MpvException e)
            {
                Console.WriteLine($"invalid error  detail: {e.Detail} msg: {e.Message}");
            }
        }

        VideoTracks = videoTrack;
        AudioTracks = audioTrack;
        SubtitleTracks = subtitleTrack;
    }


    private long? TryGetCurrentSubtitleTrack()
    {
        return (long?)Handle.TryGetProperty("current-tracks/sub/id");
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