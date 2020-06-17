using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataLayer;
using Domain;
using EbookTools;
using EbookTools.Epub;
using EbookTools.Mobi;
using Models;

namespace ElibWpf.Extensions
{
    public static class DomainExtensions
    {
        public static Book ToBook(this ParsedBook parsedBook)
        {
            using UnitOfWork uow = ApplicationSettings.CreateUnitOfWork();
            Book newBook = new Book
            {
                Title = parsedBook.Title,
                Authors = new ObservableCollection<Author>
                {
                    uow.AuthorRepository.GetAuthorWithName(parsedBook.Author) ??
                    new Author {Name = parsedBook.Author}
                },
                Cover = parsedBook.Cover != null ? ImageOptimizer.ResizeAndFill(parsedBook.Cover) : null,
                Collections = new ObservableCollection<UserCollection>(),
                File = new EFile
                {
                    Format = parsedBook.Format,
                    Signature = Signer.ComputeHash(parsedBook.RawData),
                    RawFile = new RawFile {RawContent = parsedBook.RawData}
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