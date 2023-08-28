using System;
using System.Globalization;
using System.Windows.Data;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Wpf.ViewModels.Controls;

namespace Valyreon.Elib.Wpf.Converters.BookTileConverters
{
    /// <summary>
    ///     Used to specify is tooltip enabled based on string length.
    /// </summary>
    public class BookToTileViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Book book ? new BookTileViewModel(book) : (object)null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
