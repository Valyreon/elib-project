using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DomainModel
{
    public partial class Book
    {
        DatabaseContext database = DatabaseContext.GetInstance();
        public override string ToString()
        {
            return $"ID: {id}  Name: {name} Authors: {string.Join(", ", database.GetBookAuthors(this).Select(x => x.name))}";
        }

        public string GetDetails()
        {
            const string formatString = "{0, -12} {1}\n";

            StringBuilder stringBuilder = new StringBuilder(String.Format("\n{0, -12} {1}\n", "Title:", name));
            IList<Author> authors = database.GetBookAuthors(this);
            IList<Collection> collections = database.GetBookCollections(this);

            if(authors != null)
                stringBuilder.Append(String.Format(formatString, authors.Count > 1 ? "Authors:" : "Author:", string.Join(", ", authors.Select(x => x.name))));
            if (series != null)
                stringBuilder.Append(String.Format(formatString, "Series:", series.name));
            if (series != null)
                stringBuilder.Append(String.Format(formatString, "Number:", seriesNumber));
            stringBuilder.Append(String.Format(formatString, "Read:", isRead ? "Yes" : "No"));
            if (collections != null)
                stringBuilder.Append(String.Format(formatString, collections.Count > 1 ? "Collections:" : "Collection:", string.Join(", ", collections)));
            stringBuilder.Append("\n");

            return stringBuilder.ToString();
        }
        public override bool Equals(object obj)
        {
            Book book = obj as Book;

            if (book == null)
                return false;

            return book.id == this.id;
        }
        public override int GetHashCode() => id.GetHashCode();
    }

    public partial class Author
    {
        DatabaseContext database = DatabaseContext.GetInstance();
        public override string ToString()
        {
            return $"ID: {id}  Name: {name} Books: {string.Join(", ", database.GetAuthorBooks(this).Select(x => x.name))}";
        }

        public override bool Equals(object obj)
        {
            Author author = obj as Author;

            if (author == null)
                return false;

            return author.id == this.id;
        }

        public override int GetHashCode() => id.GetHashCode();

    }

}
