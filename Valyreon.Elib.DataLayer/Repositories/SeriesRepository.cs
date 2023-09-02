using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Valyreon.Elib.DataLayer.Extensions;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Repositories
{
    public class SeriesRepository : RepositoryBase<BookSeries>, ISeriesRepository
    {
        public SeriesRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public Task<int> CountBooksInSeriesAsync(int seriesId)
        {
            return Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM Books WHERE SeriesId = @SeriesId", new { SeriesId = seriesId }, Transaction);
        }

        public async Task<BookSeries> GetByNameAsync(string name)
        {
            var cacheResult = Cache.Values.FirstOrDefault(s => s.Name == name);
            return cacheResult ?? await Connection.QueryFirstOrDefaultAsync<BookSeries>("SELECT * FROM Series WHERE Name = @Name LIMIT 1",
                new { Name = name },
                Transaction);
        }

        public async Task<IEnumerable<BookSeries>> SearchAsync(string token)
        {
            var result = await Connection.QueryAsync<BookSeries>("SELECT * FROM Series WHERE Name LIKE @Token", new { Token = $"%{token}%" }, Transaction);
            return Cache.FilterAndUpdateCache(result);
        }
    }
}
