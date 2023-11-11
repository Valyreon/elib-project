using System;
using System.Globalization;
using System.Windows.Data;

namespace Valyreon.Elib.Wpf.Converters.BookTileConverters
{
    /// <summary>
    ///     Used to specify is tooltip enabled based on string length.
    /// </summary>
    public class BoolToMarkTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool marked && parameter is string valuesCsv)
            {
                var values = valuesCsv.Split(',');
                return marked ? values[0] : values[1];
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
