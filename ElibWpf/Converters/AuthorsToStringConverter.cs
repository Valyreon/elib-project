using Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ElibWpf.Converters
{
    public class AuthorsToStringConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ICollection<Author> authorList)
            {
                return authorList.Select(a => a.Name).Aggregate((i, j) => i + ", " + j);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string allAuthors)
            {
                allAuthors.Replace(" ", "");
                List<Author> result = new List<Author>();
                foreach (var author in allAuthors.Split(','))
                {
                    result.Add(new Author { Name = author });
                }
            }
            return null;
        }
    }
}
