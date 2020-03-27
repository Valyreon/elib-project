using Domain;
using EbookTools;
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
                Cover = parsedBook.Cover,
                UserCollections = new List<UserCollection>(),
                Files = new List<EFile>{ new EFile { Format = parsedBook.Format, RawContent = parsedBook.RawData } }
            };

            return newBook;
        }
    }
}