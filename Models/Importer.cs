using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer;
using Domain;
using EbookTools;
using Models.Helpers;
using Models.Utilities;

namespace Models
{
    public class Importer
    {
        private readonly ElibContext database;

        public Importer()
        {
            database = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);
        }

        public Book ImportBook(Book book, Author author, EFile eFile, BookSeries bookSeries)
        {
            book.Authors.Add(author);
            book.Files.Add(eFile);

            if (bookSeries != null)
                book.Series = bookSeries;

            Book result = database.Books.Add(book);

            database.SaveChanges();

            return result;
        }

        public Book ImportBook(ParsedBook parsedBook, string bookName, string authorName, string seriesName, Decimal? seriesNumber)
        {
            Book book = parsedBook.GetBook();
            book.Name = bookName;

            EFile eFile = parsedBook.GetEFile();

            Author author = database.Authors.FirstOrDefault(x => x.Name == authorName);
            author ??= new Author { Name = authorName };

            BookSeries bookSeries = null;

            if(seriesNumber != null)
            {
                bookSeries = database.Series.FirstOrDefault(x => x.Name == seriesName);
                bookSeries ??= new BookSeries { Name = seriesName };

                book.NumberInSeries = seriesNumber;
            }


            return ImportBook(book, author, eFile, bookSeries);
        }

        public Task CommitChangesAsync()
        {
            return database.SaveChangesAsync();
        }

        public void CommitChanges()
        {
            database.SaveChanges();
        }
    }
}
