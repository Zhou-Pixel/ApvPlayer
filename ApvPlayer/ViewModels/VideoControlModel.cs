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

    public Mpv? Handle { set; private get; }
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
                Handle?.SetProperty("time-pos", value);
            }
            SetProperty(ref _videlValue, value);
        }
        get => _videlValue;
    }

    private double _videlValue;

    public double VolumeValue
    {
        set => Handle?.SetProperty("ao-volume", value);
        get
        {
            try
            {
                var value = (double?)Handle?.GetProperty("ao-volume");
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
            Title = "—°‘Ò ”∆µ",
            FileTypeFilter = new[]
            {
                new FilePickerFileType(" ”∆µ")
                {
                    Patterns = new []{"*.mp4", "*.mkv", "*.flv"}
                }
            },
            AllowMultiple = false

        });
        if (ret.Count == 0) return;
        var gl = control.FindControl<OpenGlControl>("GlControl");
        gl?.OpenFile(ret[0].Path.AbsolutePath);
    }


    public void MpvPropertyChanged(object sender, MpvPropertyChangedEventArgs arg)
    {
        if (arg.Spontaneous)
        {
            if (arg.MpvPropertyName == "duration")
            {
                _cacheTimePos = (double)arg.NewValue;
                VideoDuration = _cacheTimePos;
            }
            else if (arg.MpvPropertyName == "time-pos")
                VidelValue = (double)arg.NewValue;

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
        Handle?.ObserveProperty(name, format);
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