using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface ISeriesRepository : IRepository<BookSeries>
    {
        Task<BookSeries> GetByNameAsync(string name);
        Task<int> CountBooksInSeriesAsync(int seriesId);
        Task<IEnumerable<BookSeries>> SearchAsync(string token);
    }
}
