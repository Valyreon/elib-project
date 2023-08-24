using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Valyreon.Elib.Wpf.Messages;

namespace Valyreon.Elib.Wpf.Converters
{
    public class NotificationTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = ColorConverter.ConvertFromString("CornflowerBlue");
            if (value is NotificationType type)
            {
                color = type switch
                {
                    NotificationType.Info => ColorConverter.ConvertFromString("CornflowerBlue"),
                    NotificationType.Warning => ColorConverter.ConvertFromString("Coral"),
                    NotificationType.Error => ColorConverter.ConvertFromString("Crimson"),
                    NotificationType.Success => ColorConverter.ConvertFromString("SpringGreen"),
                    _ => throw new NotImplementedException(),
                };
            }

            return new SolidColorBrush((Color)color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
