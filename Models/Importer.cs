using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
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

        public Book ImportBook(ParsedBook parsedBook, string bookName, string authorName, string seriesName = null, Decimal? seriesNumber = null)
        {
            Book book = new Book()
            {
                Name = bookName,
                Authors = new List<Author> { database.Authors.Where(x => x.Name == authorName).FirstOrDefault() ?? new Author { Name = authorName } },
                Files = new List<EFile> { parsedBook.GetEFile() },
                Series = seriesName == null ? null : (database.Series.Where(x => x.Name == seriesName).FirstOrDefault() ?? new BookSeries { Name = seriesName }),
                NumberInSeries = seriesNumber
            };

            Book result = database.Books.Add(book);
            database.SaveChanges();
            return result;
        }

        public Book ImportBook(ParsedBook parsedBook)
        {
            Book book = new Book()
            {
                Name = parsedBook.Title,
                Authors = new List<Author> { database.Authors.Where(x => x.Name == parsedBook.Author).FirstOrDefault() ?? new Author { Name = parsedBook.Author } },
                Files = new List<EFile> { parsedBook.GetEFile() },
            };

            book = database.Books.Add(book);
            database.SaveChanges();
            return book;
        }

        public ICollection<Book> ImportBooksFromDirectory(string path)
        {
            ICollection<Book> result = new List<Book>();

            if (!Directory.Exists(path))
                return result;

            IEnumerable<string> validFileList = ImportUtils.GetValidBookPaths(Directory.GetFiles(path));

            foreach(string filePath in validFileList)
                result.Add(ImportBook(EbookParserFactory.Create(filePath).Parse()));

            return result;
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
