﻿using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ApvPlayer.Converters;

public class NullableObjectConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value ?? parameter;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}