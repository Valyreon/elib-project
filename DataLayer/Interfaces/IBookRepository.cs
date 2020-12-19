using System.Collections.Generic;
using Domain;

namespace DataLayer.Interfaces
{
    public interface IBookRepository : ICache<Book>
    {
        void Add(Book entity);

        IEnumerable<Book> All();

        Book Find(int id);

        IEnumerable<Book> GetBooks(IEnumerable<int> Ids);

        IEnumerable<Book> FindBySeriesId(int seriesId);

        IEnumerable<Book> FindByCollectionId(int collectionId);

        IEnumerable<Book> FindByAuthorId(int authorId);

        IEnumerable<Book> FindPageByFilter(FilterParameters filter, int offset, int pageSize);

        int Count(FilterParameters filter);

        void Remove(int id);

        void Remove(Book entity);

        void Update(Book entity);
    }
}
