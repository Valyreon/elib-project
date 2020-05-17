using System;
using System.Globalization;
using System.Windows.Data;

namespace ElibWpf.Converters.BookTileConverters
{
    /// <summary>
    ///     Used to specify is tooltip enabled based on string length.
    /// </summary>
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && parameter is string maxLengthStr)
            {
                try
                {
                    int maxLength = int.Parse(maxLengthStr);
                    if (str.Length > maxLength)
                    {
                        return true;
                    }

                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}