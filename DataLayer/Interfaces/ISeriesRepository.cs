using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Interfaces
{
    public interface ISeriesRepository : ICache<BookSeries>
    {
        void Add(BookSeries entity);

        IEnumerable<BookSeries> All();

        BookSeries GetByName(string name);

        BookSeries Find(int id);

        void Remove(int id);

        void Remove(BookSeries entity);

        void Update(BookSeries entity);

        Task AddAsync(BookSeries entity);

        Task<IEnumerable<BookSeries>> AllAsync();

        Task<BookSeries> GetByNameAsync(string name);

        Task<BookSeries> FindAsync(int id);

        Task RemoveAsync(int id);

        Task RemoveAsync(BookSeries entity);

        Task UpdateAsync(BookSeries entity);
    }
}
