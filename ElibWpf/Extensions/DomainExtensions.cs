using System;
using System.Collections.Generic;
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
            using ElibContext database = ApplicationSettings.CreateContext();
            Book newBook = new Book
            {
                Title = parsedBook.Title,
                Authors = new List<Author>
                {
                    database.Authors.FirstOrDefault(au => au.Name.Equals(parsedBook.Author)) ??
                    new Author {Name = parsedBook.Author}
                },
                Cover = parsedBook.Cover != null ? ImageOptimizer.ResizeAndFill(parsedBook.Cover) : null,
                UserCollections = new List<UserCollection>(),
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