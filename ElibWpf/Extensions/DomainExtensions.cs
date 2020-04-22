using Domain;
using EbookTools;
using EbookTools.Epub;
using EbookTools.Mobi;
using Models;
using System.Collections.Generic;
using System.Linq;

namespace ElibWpf.Extensions
{
    public static class DomainExtensions
    {
        public static Book ToBook(this ParsedBook parsedBook)
        {
            Book newBook = new Book
            {
                Title = parsedBook.Title,
                Authors = new List<Author> { App.Database.Authors.Where(au => au.Name.Equals(parsedBook.Author)).FirstOrDefault() ?? new Author() { Name = parsedBook.Author } },
                Cover = parsedBook.Cover != null ? ImageOptimizer.ResizeAndFill(parsedBook.Cover) : null,
                UserCollections = new List<UserCollection>(),
                Files = new List<EFile> { new EFile { Format = parsedBook.Format, RawContent = parsedBook.RawData } }
            };

            return newBook;
        }

        public static string GenerateHtml(this EFile book)
        {
            EbookParser parser = (book.Format) switch
            {
                ".epub" => new EpubParser(book.RawContent),
                ".mobi" => new MobiParser(book.RawContent),
                _ => throw new System.ArgumentException($"The file has an unkown extension.")
            };

            return parser.GenerateHtml();
        }
    }
}