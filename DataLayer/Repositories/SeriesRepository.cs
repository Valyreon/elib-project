using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataLayer.Extensions;
using DataLayer.Interfaces;
using Domain;

namespace DataLayer.Repositories
{
    public class SeriesRepository : RepositoryBase, ISeriesRepository
    {
        private static readonly IDictionary<int, BookSeries> cache = new Dictionary<int, BookSeries>();

        public SeriesRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public void Add(BookSeries entity)
        {
            entity.Id = Connection.ExecuteScalar<int>(
                "INSERT INTO Series(Name) VALUES (@Name); SELECT last_insert_rowid() ",
                entity,
                Transaction
            );

            cache.Add(entity.Id, entity);
        }

        public IEnumerable<BookSeries> All()
        {
            var allList = Connection.Query<BookSeries>("SELECT * FROM Series", Transaction).AsList();
            return cache.FilterAndUpdateCache(allList);
        }

        public BookSeries Find(int id)
        {
            if (cache.TryGetValue(id, out var seriesFromCache))
            {
                return seriesFromCache;
            }

            var res = Connection.QueryFirstOrDefault<BookSeries>("SELECT * FROM Series WHERE Id = @SeriesId LIMIT 1",
                new { SeriesId = id },
                Transaction);

            if (res != null)
            {
                cache.Add(res.Id, res);
            }

            return res;
        }

        public BookSeries GetByName(string name)
        {
            var cacheResult = cache.Values.FirstOrDefault(s => s.Name == name);
            return cacheResult ?? Connection.QueryFirstOrDefault<BookSeries>("SELECT * FROM Series WHERE Name = @Name LIMIT 1",
                new { Name = name },
                Transaction);
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Series WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
            cache.Remove(id);
        }

        public void Remove(BookSeries entity)
        {
            Remove(entity.Id);
            entity.Id = 0;
        }

        public void Update(BookSeries entity)
        {
            Connection.Execute("UPDATE Series SET Name = @Name WHERE Id = @Id", entity, Transaction);
        }

        public void ClearCache()
        {
            cache.Clear();
        }

        public IEnumerable<BookSeries> GetCachedObjects()
        {
            return cache.Values.ToList();
        }

        public async Task AddAsync(BookSeries entity)
        {
            entity.Id = await Connection.ExecuteScalarAsync<int>(
                "INSERT INTO Series(Name) VALUES (@Name); SELECT last_insert_rowid() ",
                entity,
                Transaction
            );

            cache.Add(entity.Id, entity);
        }

        public async Task<IEnumerable<BookSeries>> AllAsync()
        {
            var allList = await Connection.QueryAsync<BookSeries>("SELECT * FROM Series", Transaction);
            return cache.FilterAndUpdateCache(allList);
        }

        public async Task<BookSeries> GetByNameAsync(string name)
        {
            var cacheResult = cache.Values.FirstOrDefault(s => s.Name == name);
            return cacheResult ?? await Connection.QueryFirstOrDefaultAsync<BookSeries>("SELECT * FROM Series WHERE Name = @Name LIMIT 1",
                new { Name = name },
                Transaction);
        }

        public async Task<BookSeries> FindAsync(int id)
        {
            if (cache.TryGetValue(id, out var seriesFromCache))
            {
                return seriesFromCache;
            }

            var res = await Connection.QueryFirstOrDefaultAsync<BookSeries>("SELECT * FROM Series WHERE Id = @SeriesId LIMIT 1",
                new { SeriesId = id },
                Transaction);

            if (res != null)
            {
                cache.Add(res.Id, res);
            }

            return res;
        }

        public async Task RemoveAsync(int id)
        {
            await Connection.ExecuteAsync("DELETE FROM Series WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
            cache.Remove(id);
        }

        public async Task RemoveAsync(BookSeries entity)
        {
            await RemoveAsync(entity.Id);
            entity.Id = 0;
        }

        public async Task UpdateAsync(BookSeries entity)
        {
            await Connection.ExecuteAsync("UPDATE Series SET Name = @Name WHERE Id = @Id", entity, Transaction);
        }
    }
}
