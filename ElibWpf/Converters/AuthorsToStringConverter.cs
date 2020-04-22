using Domain;
using Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ElibWpf.Converters
{
    public class AuthorsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is IEnumerable<Author> authorList)
            {
                if(authorList.Count() > 0)
                    return authorList.Select(a => a.Name).Aggregate((i, j) => i + ", " + j);
            }
            Logger.Log("AUTHORS_ERROR", "AuthorList is null or count is 0. All books should have at least one author.");
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