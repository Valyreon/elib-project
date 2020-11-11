using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Domain;

namespace ElibWpf.Converters
{
    /// <summary>
    ///     This converter is used to get the placeholder image if the cover is null.
    /// </summary>
    public class ImageConverter : IValueConverter
    {
        private static readonly BitmapImage placeholder;

        static ImageConverter()
        {
            placeholder = new BitmapImage();
            placeholder.BeginInit();
            placeholder.UriSource = new Uri("pack://application:,,,/ElibWpf;component/Images/book.png");
            placeholder.EndInit();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null
                ? placeholder
                : value is Cover cover ? cover.Image : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
