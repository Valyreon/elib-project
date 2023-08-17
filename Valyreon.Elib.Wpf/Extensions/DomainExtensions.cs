using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;
using Valyreon.Elib.EbookTools.Epub;
using Valyreon.Elib.EbookTools.Mobi;
using Valyreon.Elib.EBookTools;
using Valyreon.Elib.Wpf.Models;

namespace Valyreon.Elib.Wpf.Extensions
{
    public static class DomainExtensions
    {
        public static async Task<Book> ToBookAsync(this ParsedBook parsedBook, IUnitOfWork uow)
        {
            var newBook = new Book
            {
                Title = parsedBook.Title,
                Authors = new ObservableCollection<Author>
                {
                    await uow.AuthorRepository.GetAuthorWithNameAsync(parsedBook.Author) ?? new Author { Name = parsedBook.Author }
                },
                Cover = parsedBook.Cover != null ? new Cover { Image = ImageOptimizer.ResizeAndFill(parsedBook.Cover) } : null,
                Collections = new ObservableCollection<UserCollection>(),
                Format = parsedBook.Format,
                Signature = Signer.ComputeHash(parsedBook.RawData),
                Path = string.Empty
            };

            return newBook;
        }

        public static string GenerateHtml(this Book book)
        {
            var rawContent = File.ReadAllBytes(book.Path);
            EbookParser parser = book.Format switch
            {
                ".epub" => new EpubParser(rawContent),
                ".mobi" => new MobiParser(rawContent),
                _ => throw new ArgumentException("The file has an unkown extension.")
            };

            return parser.GenerateHtml();
        }
    }
}
