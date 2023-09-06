using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer.Filters;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface ISeriesRepository : IRepository<BookSeries>
    {
        Task<BookSeries> GetByNameAsync(string name);

        Task<int> CountBooksInSeriesAsync(int seriesId);

        Task<IEnumerable<BookSeries>> SearchAsync(string token);

        Task<IEnumerable<BookSeries>> GetSeriesWithNumberOfBooks(Filter filter);
    }
}
