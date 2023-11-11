using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Valyreon.Elib.DataLayer.Filters;
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

        public Task<BookSeries> GetByNameAsync(string name)
        {
            return Connection.QueryFirstOrDefaultAsync<BookSeries>("SELECT * FROM Series WHERE Name = @Name LIMIT 1",
                new { Name = name },
                Transaction);
        }

        public async Task<IEnumerable<BookSeries>> GetSeriesWithNumberOfBooks(Filter filter)
        {
            string query;
            if (filter.CollectionId == null)
            {
                query = "SELECT * FROM Series_BookCount_View";

                if (!string.IsNullOrWhiteSpace(filter.SearchText))
                {
                    query += $" WHERE Name LIKE '%{filter.SearchText}%'";
                }
            }
            else
            {
                query = "SELECT * FROM Series_BookCount_Collections_View WHERE CollectionId = @CollectionId";

                if (!string.IsNullOrWhiteSpace(filter.SearchText))
                {
                    query += $" AND Name LIKE '%{filter.SearchText}%'";
                }
            }

            query += $" ORDER BY {(filter.SortByBookCount ? "BookCount" : "Name")} {(filter.Ascending ? "ASC" : "DESC")}";

            var result = await Connection.QueryAsync<dynamic>(query, new { filter.CollectionId }, Transaction);
            return ProcessWithBookCount(result);
        }

        public async Task<IEnumerable<BookSeries>> SearchAsync(string token)
        {
            var result = await Connection.QueryAsync<BookSeries>("SELECT * FROM Series WHERE Name LIKE @Token", new { Token = $"%{token}%" }, Transaction);
            return result;
        }

        private static IEnumerable<BookSeries> ProcessWithBookCount(IEnumerable<dynamic> dynamicResult)
        {
            foreach (var row in dynamicResult)
            {
                var fields = row as IDictionary<string, object>;

                yield return new BookSeries
                {
                    Id = (int)(long)fields["Id"],
                    Name = (string)fields["Name"],
                    NumberOfBooks = (int)(long)fields["BookCount"]
                };
            }
        }
    }
}
