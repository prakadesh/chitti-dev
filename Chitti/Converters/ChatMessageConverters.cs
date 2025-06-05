using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Chitti.Converters;

public class ChatMessageBackgroundConverter : IValueConverter
{
    public Brush UserBackground { get; set; } = new SolidColorBrush(Color.FromRgb(220, 248, 198));
    public Brush BotBackground { get; set; } = new SolidColorBrush(Color.FromRgb(232, 232, 232));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool isUser && isUser ? UserBackground : BotBackground;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ChatMessageAlignmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool isUser && isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ChatMessageForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool isError && isError ?
            new SolidColorBrush(Colors.Red) :
            new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}