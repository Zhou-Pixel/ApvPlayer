using System;
using System.Collections.Generic;
using ApvPlayer.Controls;
using ApvPlayer.Errors;
using ApvPlayer.EventArgs;
using ApvPlayer.FFI.LibMpv;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
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

    public Mpv Handle => new Mpv();

    public VideoControlModel()
    {
    }


    public double VideoDuration { set; get; } = 100;


    private double _cacheTimePos;

    public double VidelValue
    {
        set
        {
            if (Math.Abs(_cacheTimePos - value) > 0.00001 && Active)
            {
                Handle.SetProperty("time-pos", value);
            }
            _videlValue = value;
            this.RaisePropertyChanged();
        }
        get => _videlValue;
    }

    private double _videlValue;

    public double VolumeValue
    {
        set => Handle.SetProperty("ao-volume", value);
        get
        {
            try
            {
                var value = (double)Handle.GetProperty("ao-volume");
                return value * 100;
            }
            catch (MpvException e)
            {
                Console.WriteLine($"get err {e.Code}");
                return 0;
            }
        }
    }

    public bool Pause
    {
        set => Handle.SetProperty("pause", value);
        get => (bool)Handle.GetProperty("pause");
    }

    

    public bool Active => !(bool)Handle.GetProperty("idle-active");

    public async void ChooseFile(object para)
    {
        var control = (VideoControl)para;
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

        var gl = control.FindControl<OpenGlControl>("GlControl");
        var st = Uri.UnescapeDataString(ret[0].Path.AbsolutePath);
        Handle.CommandNode(new List<string>()
        {
            "loadfile",
            st
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
                break;
            case "time-pos":
                _cacheTimePos = (double)arg.NewValue;
                VidelValue = _cacheTimePos;
                break;
        }
    }

    public void ActiveProperty(string name)
    {
        var info = GetType().GetProperty(name);
        if (info == null)
        {
            throw new ArgumentException("Property not found");
        }

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