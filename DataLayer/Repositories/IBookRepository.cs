using Domain;
using System.Collections.Generic;

namespace DataLayer.Repositories
{
    public interface IBookRepository
    {
        void Add(Book entity);
        IEnumerable<Book> All();
        Book Find(int id);
        IEnumerable<Book> GetBooks(IEnumerable<int> Ids);
        IEnumerable<Book> FindBySeriesId(int seriesId);
        IEnumerable<Book> FindByCollectionId(int collectionId);
        IEnumerable<Book> FindByAuthorId(int authorId);
        IEnumerable<Book> FindPageByFilter(FilterParameters filter, int offset, int pageSize);
        void Remove(int id);
        void Remove(Book entity);
        void Update(Book entity);
        void ClearCache();
    }
}
