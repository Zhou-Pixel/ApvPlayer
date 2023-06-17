using System;
using System.Collections.Generic;
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
using ApvPlayer.Utils;
using CheckBox = ApvPlayer.Controls.CheckBox;

namespace ApvPlayer.ViewModels;

public class VideoControlModel : ViewModelBase
{


    public event Action<object>? RequestUpdateGl;
    public event Action<object, bool>? RequestFullScreen;

    public event Func<object, FilePickerOpenOptions, Task<IReadOnlyList<IStorageFile>>>? RequestOpenFile;


    private bool _playbackInitialized;

    public Mpv Handle { get; } = new();

    private readonly ISystemSetting _setting;

    public VideoControlModel()
    {
        _active = !(bool)Handle.GetProperty("idle-active");
        Handle.ObserveProperty("idle-active", Format.Flag);
        Handle.ObserveProperty("seekable", Format.Flag);
        Handle.ObserveProperty("pause", Format.Flag);
        Handle.MpvPropertyChanged += OnMpvPropertyChanged;
        Handle.MpvEventReceived += OnMpvEventReceived;
        
        SubscribeProperty();
        ActiveProperty();
        
        AddSubtitleCommand = ReactiveCommand.Create(AddSubTitle);
        SelectSubtitleCommand = ReactiveCommand.Create<MenuItem>(SelectSubtitle);
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _setting = new WindowsSystemSetting();
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private void OnMpvEventReceived(object sender, MpvEventReceivedArgs args)
    {
        if (args.Event.EventId == EventId.PlaybackRestart && !_playbackInitialized)
        {
            _setting.KeepDisplay(true);
            _playbackInitialized = true;
            Handle.SetProperty("ao-volume", _volumeValue);
            Handle.SetProperty("ao-mute", _mute);
            UpdateTracks();
        }

    }

    private void SubscribeProperty()
    {

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
            if (Active)
            {
                Handle.SetProperty("time-pos", value);
                this.RaiseAndSetIfChanged(ref _videoValue, value);
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
        set
        {
            Handle.SetProperty("pause", value);
            this.RaisePropertyChanged();
        }
        get => (bool)Handle.GetProperty("pause");
    }


    private bool _active;

    public bool Active
    {
        set => this.RaiseAndSetIfChanged(ref _active, value);
        get => _active;
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

    private bool _mute;

    public bool Mute
    {
        get => Active ? (bool)Handle.GetProperty("ao-mute") : _mute;
        set
        {
            if (Active)
                Handle.SetProperty("ao-mute", value);
            this.RaiseAndSetIfChanged(ref _mute, value);
        }
    }

    private string? _fileName;

    public string? FileName
    {
        private set => this.RaiseAndSetIfChanged(ref _fileName, value);
        get => _fileName;
    }




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


    private List<MenuItem> _subtitleMenuItems = new();

    public List<MenuItem> SubtitleMenuItems
    {
        get => _subtitleMenuItems;
        set => this.RaiseAndSetIfChanged(ref _subtitleMenuItems, value);
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
        Mute = !Mute;
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
        

        if (ret.Count == 0) return;

        var file = Uri.UnescapeDataString(ret[0].Path.AbsolutePath);

        Handle.CommandNode("loadfile", file);
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


    public void Screenshot()
    {
        Console.WriteLine(nameof(Screenshot));
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
                    _subtitleMenuItems.Clear();
                    _setting.KeepDisplay(false);
                    _playbackInitialized = false;
                    RequestUpdateGl?.Invoke(this);
                    _videoValue = 0;
                    this.RaisePropertyChanged(nameof(VideoValue));
                    FileName = (string?)Handle.TryGetProperty("filename");
                }
                //this.RaisePropertyChanged(nameof(Active));
                Console.WriteLine("idle-active change");
                break;

            }
            case "seekable":
            {
                _seekable = (bool)arg.NewValue;
                break;
            }
            case "pause":
            {
                break;
            }
            case "filename":
            {
                FileName = (string)arg.NewValue;
                break;
            }
        }
    }


    private void SelectSubtitle(MenuItem menu)
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
                }

                if (i.Item?.Icon is CheckBox checkBox)
                {
                    checkBox.IsChecked = check;
                }
            }
            
        }
        catch (MpvException e)
        {
            Console.WriteLine($"detail {e.Detail} {e.Message}");
        }
    }

    private async Task AddSubTitle()
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

    private void ActiveProperty()
    {
        Handle.ObserveProperty("filename", Format.String);
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
        
        var list = new List<MenuItem>();
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
        
        var sub = TryGetCurrentSubtitleTrack();
        long count = (long)Handle.GetProperty("track-list/count");
        for (int i = 0; i < count; i++)
        {
            try
            {

                var track = new Track()
                {
                    Id = (long)Handle.GetProperty($"track-list/{i}/id"),
                    Type = (string)Handle.GetProperty($"track-list/{i}/type"),
                    Title = (string?)Handle.TryGetProperty($"track-list/{i}/title"),
                    Lang = (string?)Handle.TryGetProperty($"track-list/{i}/lang")
                };
                Console.WriteLine($"track {track.Type} {track.Id} {track.Title} end");
                switch (track.Type)
                {
                    case "video":
                        videoTrack.Add(track);
                        break;
                    case "audio":
                        audioTrack.Add(track);
                        break;
                    case "sub":
                    {
                        var trackItem = NewMenuItem(track, sub);
                        list.Add(trackItem);
                        subtitleTrack.Add(track);
                        break;
                    }
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (MpvException e)
            {
                Console.WriteLine($"invalid error  detail: {e.Detail} msg: {e.Message}");
            }
        }

        var noneTrack = new Track()
        {
            Title = "无",
            Id = 0,
            Type = "sub"
        };
        noneTrack.Item = NewMenuItem(noneTrack, sub);
        list.Add(noneTrack.Item);
        subtitleTrack.Add(noneTrack);
        
        VideoTracks = videoTrack;
        AudioTracks = audioTrack;
        SubtitleTracks = subtitleTrack; 
        SubtitleMenuItems = list;
    }

    private MenuItem NewMenuItem(Track track, long? sub)
    {
        var item = new MenuItem()
        {
            Header = track.Lang == null ? track.Title : $"{track.Lang}  {track.Title}",
            Icon = new CheckBox()
            {
                CheckedIcon = "fa-solid fa-check",
                UnCheckedIcon = "",
                IsChecked = sub == track.Id
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
        track.Item = item;
        return item;
    }

    private long? TryGetCurrentSubtitleTrack()
    {
        return (long?)Handle.TryGetProperty("current-tracks/sub/id");
    }
}