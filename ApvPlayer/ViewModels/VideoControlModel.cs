using System;
using ApvPlayer.Controls;
using ApvPlayer.Errors;
using ApvPlayer.EventArgs;
using ApvPlayer.FFI.LibMpv;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ApvPlayer.ViewModels;

public partial class VideoControlModel : ViewModelBase
{

    private Mpv? _handle;

    public Mpv Handle
    {
        set
        {
            value.MpvPropertyChanged += MpvPropertyChanged;
            _handle = value;
        }
    }

    public VideoControlModel()
    {
    }


    [ObservableProperty]
    private double _videoDuration = 100;


    private double _cacheTimePos;

    public double VidelValue
    {
        set
        {
            if (Math.Abs(_cacheTimePos - value) > 0.00001)
            {
                _handle?.SetProperty("time-pos", value);
            }
            SetProperty(ref _videlValue, value);
        }
        get => _videlValue;
    }

    private double _videlValue;

    public double VolumeValue
    {
        set => _handle?.SetProperty("ao-volume", value);
        get
        {
            try
            {
                var value = (double?)_handle?.GetProperty("ao-volume");
                Console.WriteLine($"current value {value}");
                return value.GetValueOrDefault() * 100;
            }
            catch (MpvException e)
            {
                Console.WriteLine($"get err {e.Code}");
                return 0;
            }
        }
    }

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
        gl?.OpenFile(Uri.UnescapeDataString(ret[0].Path.AbsolutePath));
    }


    public void MpvPropertyChanged(object sender, MpvPropertyChangedEventArgs arg)
    {
        Console.WriteLine($"property change {arg.MpvPropertyName} {arg.NewValue}");
        if (arg.Spontaneous)
        {
            if (arg.MpvPropertyName == "duration")
            {
                VideoDuration = (double)arg.NewValue;
            }
            else if (arg.MpvPropertyName == "time-pos")
            {
                _cacheTimePos = (double)arg.NewValue;
                VidelValue = _cacheTimePos;
            }

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
        _handle?.ObserveProperty(name, format);
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