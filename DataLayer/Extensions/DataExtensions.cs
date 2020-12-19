using System.Collections.ObjectModel;
using Domain;

namespace DataLayer.Extensions
{
    public static class DomainExtensions
    {
        public static Book LoadMembers(this Book book, IUnitOfWork uow)
        {
            if (book.SeriesId.HasValue)
            {
                book.Series = uow.SeriesRepository.Find(book.SeriesId.Value);
            }

            book.File = uow.EFileRepository.Find(book.FileId);
            book.Collections = new ObservableCollection<UserCollection>(uow.CollectionRepository.GetUserCollectionsOfBook(book.Id));
            book.Authors = new ObservableCollection<Author>(uow.AuthorRepository.GetAuthorsOfBook(book.Id));
            if (book.CoverId.HasValue)
            {
                book.Cover = uow.CoverRepository.Find(book.CoverId.Value);
            }

            return book;
        }
    }
}
