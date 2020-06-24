using DataLayer;

namespace CommandLineInterface.Utilities
{
    public class DetailUtils
    {
        private readonly IUnitOfWork database;

        public DetailUtils(IUnitOfWork db)
        {
            this.database = db;
        }
    }

    /*public class DetailUtils
    {
        private readonly ElibContext database;

        public DetailUtils(ElibContext db)
        {
            this.database = db;
        }

        public string BookDetailsId(int bookId)
        {
            Book book = this.database.Books
                .Include("Series")
                .Include("Authors")
                .Include("Quotes")
                .Include("UserCollections")
                .FirstOrDefault(x => x.Id == bookId);

            if (book == null)
            {
                throw new KeyNotFoundException();
            }

            const string formatString = "{0, -12} {1}\n";

            StringBuilder stringBuilder = new StringBuilder(string.Format(formatString, "ID:", bookId));

            stringBuilder.Append(string.Format(formatString, "Title:", book.Title));

            var authors = book.Authors;
            var collections = book.Collections;
            BookSeries series = book.Series;

            if (authors != null)
            {
                stringBuilder.Append(string.Format(formatString, authors.Count > 1 ? "Authors:" : "Author:",
                    string.Join(", ", authors.Select(x => x.Name))));
            }

            if (series != null)
            {
                stringBuilder.Append(string.Format(formatString, "Series:", series.Name))
                    .Append(string.Format(formatString, "Number:", book.NumberInSeries));
            }

            stringBuilder.Append(string.Format(formatString, "Read:", book.IsRead ? "Yes" : "No"));

            if (collections != null && collections.Count > 0)
            {
                stringBuilder.Append(string.Format(formatString, collections.Count > 1 ? "Collections:" : "Collection:",
                    string.Join(", ", collections)));
            }

            stringBuilder.Append("\n");

            return stringBuilder.ToString();
        }

        public string AuthorDetailsId(int authorId)
        {
            Author author = this.database.Authors
                .Include("Books")
                .FirstOrDefault(x => x.Id == authorId);

            if (author == null)
            {
                throw new KeyNotFoundException();
            }

            const string formatString = "{0, -12} {1}\n";

            var books = author.Books;

            StringBuilder stringBuilder = new StringBuilder(string.Format(formatString, "Name:", author.Name));

            if (books != null)
            {
                stringBuilder.Append(string.Format(formatString, books.Count() > 1 ? "Books:" : "Book:",
                    string.Join(", ", books.Select(x => x.Title))));
            }

            return stringBuilder.ToString();
        }

        public string UserCollectionDetailsId(int collectionId)
        {
            UserCollection collection = this.database.UserCollections
                .Include("Books")
                .FirstOrDefault(x => x.Id == collectionId);

            if (collection == null)
            {
                throw new KeyNotFoundException();
            }

            const string formatString = "{0, -12} {1}\n";

            var books = collection.Books;

            StringBuilder stringBuilder = new StringBuilder(string.Format(formatString, "Name:", collection.Tag));

            if (books != null)
            {
                stringBuilder.Append(string.Format(formatString, books.Count > 1 ? "Books:" : "Book:",
                    string.Join(", ", books.Select(x => x.Title))));
            }

            return stringBuilder.ToString();
        }

        public string BookSeriesDetailsId(int seriesId)
        {
            BookSeries series = this.database.Series
                .Include("Books")
                .FirstOrDefault(x => x.Id == seriesId);

            if (series == null)
            {
                throw new KeyNotFoundException();
            }

            const string formatString = "{0, -12} {1}\n";

            var books = series.Books;

            StringBuilder stringBuilder = new StringBuilder(string.Format(formatString, "Name:", series.Name));

            if (books != null)
            {
                stringBuilder.Append(string.Format(formatString, books.Count() > 1 ? "Books:" : "Book:",
                    string.Join(", ", books.Select(x => x.Title))));
            }

            return stringBuilder.ToString();
        }
    }*/
}