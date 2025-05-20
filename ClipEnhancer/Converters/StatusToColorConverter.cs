using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ClipEnhancer.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.ToLower() switch
            {
                "success" => new SolidColorBrush(Color.FromRgb(34, 197, 94)), // SuccessGreen
                "error" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),   // ErrorRed
                _ => new SolidColorBrush(Color.FromRgb(102, 102, 102))        // SecondaryText
            };
        }
        return new SolidColorBrush(Color.FromRgb(102, 102, 102));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 