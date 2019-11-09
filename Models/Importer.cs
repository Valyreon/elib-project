using DataLayer;
using Domain;
using EbookTools;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class Importer
    {
        private readonly ElibContext database;

        public Importer(ElibContext db) => database = db;

        public async Task<Book> ImportBook(ParsedBook parsedBook, string bookName, string authorName, string seriesName = null, decimal? seriesNumber = null)
        {
            Book book = new Book()
            {
                Title = bookName,
                Authors = new List<Author> { database.Authors.Where(x => x.Name == authorName).FirstOrDefault() ?? new Author { Name = authorName } },
                Files = new List<EFile>
                {
                    new EFile(parsedBook.Format, parsedBook.RawData)
                },
                Series = string.IsNullOrEmpty(seriesName) ? null : (database.Series.Where(x => x.Name == seriesName).FirstOrDefault() ?? new BookSeries { Name = seriesName }),
                NumberInSeries = seriesNumber
            };

            Book result = database.Books.Add(book);
            await database.SaveChangesAsync();
            return result;
        }

        public async Task<Book> ImportBook(ParsedBook parsedBook)
        {
            Book book = new Book()
            {
                Title = parsedBook.Title,
                Authors = new List<Author> { database.Authors.Where(x => x.Name == parsedBook.Author).FirstOrDefault() ?? new Author { Name = parsedBook.Author } },
                Files = new List<EFile>
                {
                    new EFile(parsedBook.Format, parsedBook.RawData)
                }
            };

            book = database.Books.Add(book);
            await database.SaveChangesAsync();
            return book;
        }

        public async Task<ICollection<Book>> ImportBooksFromDirectory(string path)
        {
            ICollection<Book> result = new List<Book>();

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException();

            List<string> validFileList = new List<string>();
            foreach (string ext in EbookParserFactory.SupportedExtensions)
                validFileList.AddRange(Directory.GetFiles(path, $"*{ext}"));

            foreach (string filePath in validFileList)
                result.Add(await ImportBook(EbookParserFactory.Create(filePath).Parse()));

            return result;
        }
    }
}