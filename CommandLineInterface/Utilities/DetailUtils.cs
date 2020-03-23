using DataLayer;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommandLineInterface.Utilities
{
    public class DetailUtils
    {
        private readonly ElibContext _database;

        public DetailUtils(ElibContext db)
        {
            this._database = db;
        }

        public string BookDetailsID(int book_id)
        {
            Book book = _database.Books
                .Include("Series")
                .Include("Authors")
                .Include("Quotes")
                .Include("UserCollections")
                .Where(x => x.Id == book_id)
                .FirstOrDefault();

            if (book == null)
                throw new KeyNotFoundException();

            const string formatString = "{0, -12} {1}\n";

            StringBuilder stringBuilder = new StringBuilder(String.Format(formatString, "ID:", book_id));

            stringBuilder.Append(String.Format(formatString, "Title:", book.Title));

            ICollection<Author> authors = book.Authors;
            ICollection<UserCollection> collections = book.UserCollections;
            BookSeries series = book.Series;

            if (authors != null)
                stringBuilder.Append(string.Format(formatString, authors.Count > 1 ? "Authors:" : "Author:", string.Join(", ", authors.Select(x => x.Name))));

            if (series != null)
                stringBuilder.Append(string.Format(formatString, "Series:", series.Name))
                    .Append(string.Format(formatString, "Number:", book.NumberInSeries));

            stringBuilder.Append(string.Format(formatString, "Read:", (bool)book.IsRead ? "Yes" : "No"));

            if (collections != null && collections.Count > 0)
                stringBuilder.Append(string.Format(formatString, collections.Count > 1 ? "Collections:" : "Collection:", string.Join(", ", collections)));

            stringBuilder.Append("\n");

            return stringBuilder.ToString();
        }

        public string AuthorDetailsID(int author_id)
        {
            Author author = _database.Authors
                   .Include("Books")
                   .Where(x => x.Id == author_id)
                   .FirstOrDefault();

            if (author == null)
                throw new KeyNotFoundException();

            const string formatString = "{0, -12} {1}\n";

            ICollection<Book> books = author.Books;

            StringBuilder stringBuilder = new StringBuilder(string.Format(formatString, "Name:", author.Name));

            if (books != null)
                stringBuilder.Append(string.Format(formatString, books.Count > 1 ? "Books:" : "Book:", string.Join(", ", books.Select(x => x.Title))));

            return stringBuilder.ToString();
        }

        public string UserCollectionDetailsID(int collection_id)
        {
            UserCollection collection = _database.UserCollections
                   .Include("Books")
                   .Where(x => x.Id == collection_id)
                   .FirstOrDefault();

            if (collection == null)
                throw new KeyNotFoundException();

            const string formatString = "{0, -12} {1}\n";

            ICollection<Book> books = collection.Books;

            StringBuilder stringBuilder = new StringBuilder(string.Format(formatString, "Name:", collection.Tag));

            if (books != null)
                stringBuilder.Append(string.Format(formatString, books.Count > 1 ? "Books:" : "Book:", string.Join(", ", books.Select(x => x.Title))));

            return stringBuilder.ToString();
        }

        public string BookSeriesDetailsID(int series_id)
        {
            BookSeries series = _database.Series
                   .Include("Books")
                   .Where(x => x.Id == series_id)
                   .FirstOrDefault();

            if (series == null)
                throw new KeyNotFoundException();

            const string formatString = "{0, -12} {1}\n";

            ICollection<Book> books = series.Books;

            StringBuilder stringBuilder = new StringBuilder(string.Format(formatString, "Name:", series.Name));

            if (books != null)
                stringBuilder.Append(string.Format(formatString, books.Count > 1 ? "Books:" : "Book:", string.Join(", ", books.Select(x => x.Title))));

            return stringBuilder.ToString();
        }
    }
}