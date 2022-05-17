﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace VolumeControl.WPF
{
    /// <summary>
    /// Converts from any object type to bool depending on whether or not that object is null.
    /// </summary>
    /// <remarks>Returns true when the object is null.</remarks>
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value == null;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value == null;
    }
}