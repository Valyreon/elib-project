using DataLayer;
using Domain;
using EbookTools;
using System.Collections.Generic;
using System.IO;

namespace Models
{
    public class Importer
    {
        private readonly IUnitOfWork database;

        public Importer(IUnitOfWork uow)
        {
            this.database = uow;
        }

        public Book ImportBook(ParsedBook parsedBook, string bookName, string authorName, string seriesName = null,
            decimal? seriesNumber = null)
        {
            Book book = new Book
            {
                Title = bookName,
                /*Authors = new List<Author>
                {
                    this.database.Authors.FirstOrDefault(x => x.Name == authorName) ?? new Author {Name = authorName}
                },*/
                File = new EFile
                {
                    Format = parsedBook.Format,
                    Signature = Signer.ComputeHash(parsedBook.RawData),
                    RawFile = new RawFile { RawContent = parsedBook.RawData }
                },
                Series = string.IsNullOrEmpty(seriesName)
                    ? null
                    : database.SeriesRepository.GetByName(seriesName) ??
                      new BookSeries { Name = seriesName },
                NumberInSeries = seriesNumber,
                Cover = new Cover { Image = ImageOptimizer.ResizeAndFill(parsedBook.Cover) }
            };

            this.database.BookRepository.Add(book);
            return book;
        }

        public Book ImportBook(ParsedBook parsedBook)
        {
            Book book = new Book
            {
                Title = parsedBook.Title,
                /*Authors = new List<Author>
                {
                    this.database.Authors.FirstOrDefault(x => x.Name == parsedBook.Author) ??
                    new Author {Name = parsedBook.Author}
                },*/
                File = new EFile
                {
                    Format = parsedBook.Format,
                    Signature = Signer.ComputeHash(parsedBook.RawData),
                    RawFile = new RawFile { RawContent = parsedBook.RawData }
                },
                Cover = new Cover { Image = ImageOptimizer.ResizeAndFill(parsedBook.Cover) }
            };

            this.database.BookRepository.Add(book);
            return book;
        }

        public ICollection<Book> ImportBooksFromDirectory(string path)
        {
            ICollection<Book> result = new List<Book>();

            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException();
            }

            var validFileList = new List<string>();
            foreach (string ext in EbookParserFactory.SupportedExtensions)
            {
                validFileList.AddRange(Directory.GetFiles(path, "*" + ext));
            }

            foreach (string filePath in validFileList)
            {
                result.Add(this.ImportBook(EbookParserFactory.Create(filePath).Parse()));
            }

            return result;
        }
    }
}