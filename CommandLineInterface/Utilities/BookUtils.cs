using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLineInterface.Utilities
{
    public class BookUtils
    {
        public static string GetDetails(Book book)
        {
            const string formatString = "{0, -12} {1}\n";

            StringBuilder stringBuilder = new StringBuilder(String.Format(formatString, "Title:", book.Name));

            ICollection<Author> authors = book.Authors;
            ICollection<UserCollection> collections = book.UserCollections;
            BookSeries series = book.Series;

            if (authors != null)
                stringBuilder.Append(String.Format(formatString, authors.Count > 1 ? "Authors:" : "Author:", string.Join(", ", authors.Select(x => x.Name))));

            if (series != null)
                stringBuilder.Append(String.Format(formatString, "Series:", series.Name))
                    .Append(String.Format(formatString, "Number:", book.NumberInSeries));

            if(book.IsRead != null)
                stringBuilder.Append(String.Format(formatString, "Read:", (bool)book.IsRead ? "Yes" : "No"));

            if (collections != null)
                stringBuilder.Append(String.Format(formatString, collections.Count > 1 ? "Collections:" : "Collection:", string.Join(", ", collections)));

            stringBuilder.Append("\n\n");

            return stringBuilder.ToString();
        }
    }
}
