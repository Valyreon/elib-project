using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Valyreon.Elib.BookDataAPI;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;
using Valyreon.Elib.EBookTools;
using Valyreon.Elib.Wpf.Models;

namespace Valyreon.Elib.Wpf.Extensions
{
    public static class DomainExtensions
    {
        public static void Clean(this Book book)
        {
            book.Title = book.Title.Trim();
        }

        public static async Task Fill(this Book book, BookInformation info, IUnitOfWorkFactory uowFactory)
        {
            book.Title = info.Title.Trim();
            book.Description = info.Description.Trim();

            book.Cover ??= new Cover();
            book.Cover.Image = ImageOptimizer.ResizeAndFill(info.Cover);

            var newAuthors = new ObservableCollection<Author>();
            foreach (var authName in info.Authors)
            {
                var alreadyAdded = book.Authors.FirstOrDefault(a => a.Name == authName);
                if (alreadyAdded != null)
                {
                    newAuthors.Add(alreadyAdded);
                }
                else
                {
                    using var uow = await uowFactory.CreateAsync();
                    var authorInDb = await uow.AuthorRepository.GetAuthorWithNameAsync(authName);
                    newAuthors.Add(authorInDb ?? new Author { Name = authName });
                }
            }

            book.Authors = newAuthors;
        }

        public static bool IsValid(this Book book)
        {
            if (book == null)
            {
                return false;
            }

            return book.Title.IsDefined() && book.Authors?.Count > 0 && book.Authors.All(a => a.Name.IsDefined())
                && book.Path.IsDefined() && book.Signature.IsDefined() && (book.Series?.Name.IsDefined() != false);
        }

        public static async Task LoadBookAsync(this Book book, IUnitOfWork uow)
        {
            if (book.IsLoaded)
            {
                return;
            }

            book.Authors = new ObservableCollection<Author>(await uow.AuthorRepository.GetAuthorsOfBookAsync(book.Id));
            book.Collections = new ObservableCollection<UserCollection>(await uow.CollectionRepository.GetUserCollectionsOfBookAsync(book.Id));

            if (book.SeriesId.HasValue)
            {
                book.Series = await uow.SeriesRepository.FindAsync(book.SeriesId.Value);
            }

            if (book.CoverId.HasValue)
            {
                book.Cover = await uow.CoverRepository.FindAsync(book.CoverId.Value);
            }

            book.IsLoaded = true;
        }

        public static async Task<Book> ToBookAsync(this ParsedBook parsedBook, IUnitOfWorkFactory uowFactory)
        {
            var newBook = new Book
            {
                Title = parsedBook.Title,
                Authors = new ObservableCollection<Author>(),
                Cover = parsedBook.Cover != null ? new Cover { Image = ImageOptimizer.ResizeAndFill(parsedBook.Cover) } : null,
                Collections = new ObservableCollection<UserCollection>(),
                Format = Path.GetExtension(parsedBook.Path),
                Signature = Signer.ComputeHash(parsedBook.Path),
                Path = parsedBook.Path,
                Description = parsedBook.Description,
                ISBN = parsedBook.Isbn
            };

            if (parsedBook.Authors == null)
            {
                return newBook;
            }

            foreach (var author in parsedBook.Authors)
            {
                using var uow = await uowFactory.CreateAsync();
                newBook.Authors.Add(await uow.AuthorRepository.GetAuthorWithNameAsync(author) ?? new Author { Name = author });
            }

            return newBook;
        }
    }
}
