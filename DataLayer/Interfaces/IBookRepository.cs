using System.Collections.Generic;
using System.Threading.Tasks;
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

        Task AddAsync(Book entity);

        Task<IEnumerable<Book>> AllAsync();

        Task<Book> FindAsync(int id);

        Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<int> Ids);

        Task<IEnumerable<Book>> FindBySeriesIdAsync(int seriesId);

        Task<IEnumerable<Book>> FindByCollectionIdAsync(int collectionId);

        Task<IEnumerable<Book>> FindByAuthorIdAsync(int authorId);

        Task<IEnumerable<Book>> FindPageByFilterAsync(FilterParameters filter, int offset, int pageSize);

        Task<int> CountAsync(FilterParameters filter);

        Task RemoveAsync(int id);

        Task RemoveAsync(Book entity);

        Task UpdateAsync(Book entity);
    }
}
