using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer.Filters;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface ISeriesRepository : IRepository<BookSeries>
    {
        Task<int> CountBooksInSeriesAsync(int seriesId);

        Task<BookSeries> GetByNameAsync(string name);

        Task<IEnumerable<BookSeries>> GetSeriesWithNumberOfBooks(Filter filter);

        Task<IEnumerable<BookSeries>> SearchAsync(string token);
    }
}
