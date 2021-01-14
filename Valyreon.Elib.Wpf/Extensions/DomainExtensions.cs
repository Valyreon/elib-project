using System;
using System.Collections.ObjectModel;
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
        public static Book ToBook(this ParsedBook parsedBook, IUnitOfWork uow)
        {
            var newBook = new Book
            {
                Title = parsedBook.Title,
                Authors = new ObservableCollection<Author>
                {
                    uow.AuthorRepository.GetAuthorWithName(parsedBook.Author) ?? new Author {Name = parsedBook.Author}
                },
                Cover = parsedBook.Cover != null ? new Cover { Image = ImageOptimizer.ResizeAndFill(parsedBook.Cover) } : null,
                Collections = new ObservableCollection<UserCollection>(),
                File = new EFile
                {
                    Format = parsedBook.Format,
                    Signature = Signer.ComputeHash(parsedBook.RawData),
                    RawFile = new RawFile { RawContent = parsedBook.RawData }
                }
            };

            return newBook;
        }

        public static string GenerateHtml(this EFile book)
        {
            EbookParser parser = book.Format switch
            {
                ".epub" => new EpubParser(book.RawFile.RawContent),
                ".mobi" => new MobiParser(book.RawFile.RawContent),
                _ => throw new ArgumentException("The file has an unkown extension.")
            };

            return parser.GenerateHtml();
        }
    }
}
