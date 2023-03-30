using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ApvPlayer.Controls;
using ApvPlayer.Errors;
using ApvPlayer.EventArgs;
using ApvPlayer.FFI.LibMpv;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Win32.DirectX;
using ReactiveUI;

namespace ApvPlayer.ViewModels;

public partial class VideoControlModel : ViewModelBase
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

    public Mpv Handle { get; } = new();

    public VideoControlModel()
    {
        
        _active = !(bool)Handle.GetProperty("idle-active");
        Handle.ObserveProperty("idle-active", MpvFormat.MpvFormatFlag);
        Handle.MpvPropertyChanged += MpvPropertyChanged;
    }


    private double _videoDuration = 100;

    public double VideoDuration
    {
        set
        {
            _videoDuration = value;
            this.RaisePropertyChanged();
        }
        get => _videoDuration;
    }


    public double VidelValue
    {
        set
        {
            _videlValue = value;
            if (Active)
            {
                Handle.SetProperty("time-pos", value);
            }
            // this.RaisePropertyChanged();
        }
        get => _videlValue;
    }

    private double _videlValue;

    public double VolumeValue
    {
        set => Handle.SetProperty("ao-volume", value);
        get => _active ? (double)Handle.GetProperty("ao-volume") : 0;
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
        set
        {
            _active = value;
            this.RaisePropertyChanged();
        }
        get => _active;
    }

    public async Task ChooseFile(VideoControl control)
    {
        // var control = (VideoControl)para;
        var window = TopLevel.GetTopLevel(control);
        if (window == null) return;
        var ret = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "选择视频",
            FileTypeFilter = new[]
            {
                new FilePickerFileType("视频")
                {
                    Patterns = new []{"*.mp4", "*.mkv", "*.flv"}
                }
            },
            AllowMultiple = false

        });
        if (ret.Count == 0) return;

        var file = Uri.UnescapeDataString(ret[0].Path.AbsolutePath);




        Handle.CommandNode(new List<string>()
        {
            "loadfile",
            file
        });
        //_handle?.CommandNode("loadfile", st);
        //gl?.OpenFile(st);
    }


    public void MpvPropertyChanged(object sender, MpvPropertyChangedEventArgs arg)
    {
        if (!arg.Spontaneous) return;
        switch (arg.MpvPropertyName)
        {
            case "duration":
                VideoDuration = (double)arg.NewValue;
                Console.WriteLine($"duration ==> {_videoDuration}");
                break;
            case "time-pos":
                var value = (double)arg.NewValue;
                if (Math.Abs(_videlValue - value) > 0.00001)
                {
                    _videlValue = value;
                    this.RaisePropertyChanged(nameof(VidelValue));
                }
                
                
                // _cacheTimePos = (double)arg.NewValue;
                // VidelValue = _cacheTimePos;
                Console.WriteLine($"time-pos: ==> {_videlValue}");
                break;
            case "idle-active":
                Active = !(bool)arg.NewValue; 
                break;
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

    public void SwitchState()
    {
        Pause = !Pause;
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