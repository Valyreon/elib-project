using System;
using System.Globalization;
using System.Windows.Data;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.Wpf.Converters
{
    public class ObservableEntityToIsNewBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableEntity entity)
            {
                return entity.Id == 0;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
