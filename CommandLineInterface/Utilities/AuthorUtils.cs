using Domain;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommandLineInterface.Utilities
{
    public class AuthorUtils
    {
        public static string GetDetails(Author author)
        {
            const string formatString = "{0, -12} {1}\n";

            ICollection<Book> books = author.Books;

            StringBuilder stringBuilder = new StringBuilder(string.Format(formatString, "Name:", author.Name));

            if (books != null)
                stringBuilder.Append(string.Format(formatString, books.Count > 1 ? "Books:" : "Book:", string.Join(", ", books.Select(x => x.Name))));

            return stringBuilder.ToString();
        }
    }
}