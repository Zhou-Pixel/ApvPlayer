using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ApvPlayer.Errors;
using ApvPlayer.EventArgs;
using ApvPlayer.FFI.LibMpv;
using ApvPlayer.Views;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;
using ReactiveUI;
using ApvPlayer.Utils.Configueration;
using ApvPlayer.Utils.System;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using CheckBox = ApvPlayer.Controls.CheckBox;

namespace ApvPlayer.ViewModels;

public class VideoControlModel : ViewModelBase
{

    public event Action<object>? RequestUpdateGl;
    public event Action<object, bool>? RequestFullScreen;

    public event Func<object, FilePickerOpenOptions, Task<IReadOnlyList<IStorageFile>>>? RequestOpenFile;


    private bool _playbackInitialized;

    public Mpv Handle { get; } = new();

    private readonly ISystemSetting _systemSetting;
    private readonly Config _config = Config.Instance;

    private WindowNotificationManager? _manager;

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
        
        AddSubtitleCommand = ReactiveCommand.Create<MenuItem, Task>(AddSubTitle);
        SelectSubtitleCommand = ReactiveCommand.Create<MenuItem, Task>(SelectSubtitle);
        
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _systemSetting = new WindowsSystemSetting();
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
            _systemSetting.KeepDisplay(true);
            _playbackInitialized = true;
            Handle.SetProperty("ao-volume", _volumeValue);
            Handle.SetProperty("ao-mute", _mute);
            UpdateTracks();
        }
        Console.WriteLine($"event is {args.Event.EventId} {args.Event.ReplyUserData}  {args.Event.Error}");
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

        this.WhenAnyValue(o => o.Subtitles)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(SubtitleItems)));

    }
    
    
    public ReactiveCommand<MenuItem, Task> AddSubtitleCommand { get; }

    public ReactiveCommand<MenuItem, Task> SelectSubtitleCommand { get; }

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
            if (!Active) return;
            Handle.SetProperty("time-pos", value);
            this.RaiseAndSetIfChanged(ref _videoValue, value);

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


    private bool Stoped { set; get; }

    private CancellationTokenSource? PointerToken { set; get; }

    public string VideoFormat => (string)Handle.GetProperty("video-format");


    public ObservableCollection<string> PlayList { get; set; } = new()
    {
        "1",
        "second"
    };

    private ObservableCollection<SubtitleModel> _subtitles = new();

    public ObservableCollection<SubtitleModel> Subtitles
    {
        get => _subtitles;
        set => this.RaiseAndSetIfChanged(ref _subtitles, value);
    }

    public ObservableCollection<MenuItem> SubtitleItems
    {
        get
        {
            ObservableCollection<MenuItem> items = new();
            foreach (var i in Subtitles)
            {
                var item = new MenuItem()
                {
                    Header = i.Header,
                    Command = i.Command,
                    Icon = i.Icon
                };
                item.CommandParameter = item;
                Attached.SetId(item, i.Id);
                items.Add(item);
            }

            return items;
        }
    }

    public async Task SwitchPause()
    {
        if (Active)
        {
            Pause = !Pause;
        }
        else if (!_config.History.Records.Any())
        {
            await OpenLocalFileVideo();
        }
        else
        {
            var record = _config.History.Records.Last();
            
            Dictionary<string, object> options = new()
            {
                {"start", record.Position.ToString("f2")}
            };
            await Handle.CommandNodeAsync("loadfile", record.Path, "replace", options);
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


    public async Task Unloaded()
    {
        await Stop();
    }

    public async Task PointerMoved(UserControl control)
    {
        control.Cursor = new Cursor(StandardCursorType.Arrow);
        
        if (Active)
        {
            PointerToken?.Cancel();
            PointerToken = new CancellationTokenSource();
            
            if (FullScreen && IsPointerOverInnerBar)
                return;
            try
            {
                await Task.Delay(3000, PointerToken.Token);
                control.Cursor = new Cursor(StandardCursorType.None);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }

    public async Task OpenLocalFileVideo()
    {
        if (RequestOpenFile == null) return;
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

        SingleRecord? record = null;
        var fileUri = new Uri(file);
        foreach (var i in _config.History.Records.Where(i => new Uri(i.Path) == fileUri))
        {
            record = i;
        }
        if (record == null)
        {
            await Handle.CommandNodeAsync("loadfile", file);
            _config.AddRecord(new SingleRecord
            {
                Finished = false,
                Path = file,
                Position = 0,
                Guid = Guid.NewGuid().ToString(),
                Time = DateTime.Now
            });
        }
        else
        {
            Dictionary<string, object> options = new()
            {
                { "start", record.Position.ToString("f2") }
            };
            Console.WriteLine($"open file pos {file}");
            await Handle.CommandNodeAsync("loadfile", file, "replace", options);
            record.Time = DateTime.Now;
        }
        await _config.RefleshHistory();
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


    public async Task Screenshot(UserControl control)
    {
        // Console.WriteLine(nameof(Screenshot));
        if (!Active) return;
        _manager ??= new WindowNotificationManager(TopLevel.GetTopLevel(control)) { MaxItems = 3 };
        try
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var pic = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{path}\\ApvPlayer_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fff}.jpg" : $"{path}/ApvPlayer_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fff}.jpg";
            await Handle.CommandNodeAsync("screenshot-to-file", pic);
            Notification notification = new Notification("截图", pic, NotificationType.Success, onClick: () =>
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo(pic)
                    {
                        UseShellExecute = true
                    }
                };
                process.Start();
            });
            _manager?.Show(notification);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    

    private async Task OnMpvPropertyChanged(object sender, MpvPropertyChangedEventArgs arg)
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
                var oldValue = Active;
                Active = !(bool)arg.NewValue;
                if (!Active && oldValue)
                {
                    await Finished();
                }
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


    private async Task SelectSubtitle(MenuItem item)
    {
        try
        {
            var id = Attached.GetId(item);
            if (id == null)
                return;
            Console.WriteLine("select sub");
            await Handle.SetPropertyAsync("sid", id);
            Console.WriteLine("select sub end");
            foreach (var i in Subtitles)
            {
                if (i.Icon is CheckBox checkBox)
                {
                    checkBox.IsChecked = Equals(i.Icon, item.Icon);
                }
            }
        }
        catch (MpvException e)
        {
            Console.WriteLine($"detail {e.Detail} {e.Message}");
        }
    }

    private async Task AddSubTitle(MenuItem item)
    {
        if (!Active || RequestOpenFile == null) return; 
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) file = @"\\?\" + file;

        var node = await Handle.CommandNodeAsync("sub-add", file);
        if (node != null)
        {
            var no = Node.FromObject(node);
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

    public async Task Stop()
    {
        if (!Active) return;
        Stoped = true;
        await Handle.CommandNodeAsync("stop", "keep-playlist");
    }
    #endregion

    private void UpdateTracks()
    {
        var subtitles = new ObservableCollection<SubtitleModel>
        {
            new()
            {
                Header = "添加字幕",
                Command = AddSubtitleCommand,
                Icon = new CheckBox
                {
                    CheckedIcon = string.Empty,
                    UnCheckedIcon = string.Empty
                }
            },
            new()
            {
                Id = 0,
                Header = "无",
                Command = SelectSubtitleCommand,
                Icon = new CheckBox
                {
                    CheckedIcon = "fa-solid fa-check",
                    UnCheckedIcon = string.Empty
                }
            }
        };
        
        var sub = TryGetCurrentSubtitleTrack();
        long count = (long)Handle.GetProperty("track-list/count");
        for (int i = 0; i < count; i++)
        {
            try
            {
                var id = (long)Handle.GetProperty($"track-list/{i}/id");
                var type = (string)Handle.GetProperty($"track-list/{i}/type");
                var title = (string?)Handle.TryGetProperty($"track-list/{i}/title");
                var lang = (string?)Handle.TryGetProperty($"track-list/{i}/lang");
                switch (type)
                {
                    case "video":
                        break;
                    case "audio":
                        break;
                    case "sub":
                    {
                        subtitles.Add(new SubtitleModel
                        {
                            Id = id,
                            Header =  $"{lang}{(lang != null && title != null ? "-" : string.Empty)}{title}",
                            Icon = new CheckBox
                            {
                                CheckedIcon = "fa-solid fa-check",
                                UnCheckedIcon = string.Empty,
                                IsChecked = sub == id
                            },
                            Command = SelectSubtitleCommand
                        });
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

        Subtitles = subtitles;
    }


    private long? TryGetCurrentSubtitleTrack()
    {
        return (long?)Handle.TryGetProperty("current-tracks/sub/id");
    }

    private async Task Finished()
    {
        Console.WriteLine("!Active");
        if (Stoped)
        {
            var record = _config.History.Records.Last();
            record.Position = VideoValue;
        }
        else
        {
            var record = _config.History.Records.Last();
            record.Position = 0;
            record.Finished = true;
        }
        Stoped = false;
        Subtitles.Clear();
        _systemSetting.KeepDisplay(false);
        _playbackInitialized = false;
        RequestUpdateGl?.Invoke(this);
        _videoValue = 0;
        this.RaisePropertyChanged(nameof(VideoValue));
        await _config.RefleshHistory();
        
    }
}