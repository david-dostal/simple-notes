using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SimpleNotes.Converters
{
    [ValueConversion(typeof(bool), typeof(TextWrapping))]
    public class BooleanToTextWrappingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => ((bool)value) ? TextWrapping.Wrap : TextWrapping.NoWrap;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => ((TextWrapping)value) == TextWrapping.NoWrap ? false : true;
    }
}
