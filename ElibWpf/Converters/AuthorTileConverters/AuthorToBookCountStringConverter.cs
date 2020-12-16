using System;
using System.Globalization;
using System.Windows.Data;
using Domain;
using ElibWpf.Models;

namespace ElibWpf.Converters.AuthorTileConverters
{
    public class AuthorToBookCountStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Author str)
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                var count = uow.AuthorRepository.CountBooksByAuthor(str.Id);

                return $"{count } book{(count > 1 ? "s" : "")}";
            }

            return "No books";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
