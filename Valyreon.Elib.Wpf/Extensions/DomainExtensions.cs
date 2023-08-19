using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;
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
                Authors = new ObservableCollection<Author>(),
                Cover = parsedBook.Cover != null ? new Cover { Image = ImageOptimizer.ResizeAndFill(parsedBook.Cover) } : null,
                Collections = new ObservableCollection<UserCollection>(),
                Format = Path.GetExtension(parsedBook.Path),
                Signature = Signer.ComputeHash(parsedBook.Path),
                Path = parsedBook.Path
            };

            if (parsedBook.Authors == null)
            {
                return newBook;
            }

            foreach(var author in parsedBook.Authors)
            {
                newBook.Authors.Add(await uow.AuthorRepository.GetAuthorWithNameAsync(author) ?? new Author { Name = author });
            }

            return newBook;
        }
    }
}
