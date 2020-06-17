using Domain;
using System.Collections.Generic;

namespace DataLayer.Repositories
{
    public interface IBookRepository
    {
        void Add(Book entity);
        IEnumerable<Book> All();
        Book Find(int id);
        IEnumerable<Book> FindBySeriesId(int seriesId);
        IEnumerable<Book> FindByCollectionId(int collectionId);
        IEnumerable<Book> FindByAuthorId(int authorId);
        IEnumerable<Book> FindPageByFilter(FilterAlt filter, Book lastValueInPage);
        IEnumerable<Book> GetPage(int lastId);
        void Remove(int id);
        void Remove(Book entity);
        void Update(Book entity);
    }
}
