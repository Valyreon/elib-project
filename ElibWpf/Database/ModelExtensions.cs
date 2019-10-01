using ElibWpf.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.DomainModel
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
            StringBuilder stringBuilder = new StringBuilder(String.Format("\n{0, -12} {1}\n", "Title:", name));
            IList<Author> authors = database.GetBookAuthors(this);
            IList<Collection> collections = database.GetBookCollections(this);
            if(authors != null)
                stringBuilder.Append(String.Format("{0, -12} {1}\n", authors.Count > 1 ? "Authors:" : "Author:", string.Join(", ", authors.Select(x => x.name))));
            if (series != null)
                stringBuilder.Append(String.Format("{0, -12} {1}\n", "Series:", series.name));
            if (series != null)
                stringBuilder.Append(String.Format("{0, -12} {1}\n", "Number:", seriesNumber));

                stringBuilder.Append(String.Format("{0, -12} {1}\n", "Read:", isRead ? "Yes" : "No"));

            if (collections != null)
                stringBuilder.Append(String.Format("{0, -12} {1}\n\n", collections.Count > 1 ? "Collections:" : "Collection:", string.Join(", ", collections)));

            return stringBuilder.ToString();
        }
    }

    public partial class Author
    {
        DatabaseContext database = DatabaseContext.GetInstance();
        public override string ToString()
        {
            return $"ID: {id}  Name: {name} Books: {string.Join(", ", database.GetAuthorBooks(this).Select(x => x.name))}";
        }
    }


}
