namespace MauiProject.Converters;
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

public class NullToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != null;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}