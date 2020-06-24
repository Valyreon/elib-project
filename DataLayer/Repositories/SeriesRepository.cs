using Dapper;
using Domain;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataLayer.Repositories
{
    public class SeriesRepository : RepositoryBase, ISeriesRepository
    {
        private static readonly List<BookSeries> cache = new List<BookSeries>();

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

            cache.Add(entity);
        }

        public IEnumerable<BookSeries> All()
        {
            var allList = Connection.Query<BookSeries>("SELECT * FROM Series", Transaction).AsList();

            foreach (var series in allList)
            {
                var itemInCache = cache.Find(x => x.Id == series.Id);
                if (itemInCache == null)
                    cache.Add(series);
                else
                    itemInCache.Name = series.Name;
            }

            return cache.ToList();
        }

        public void CleanSeries()
        {
            var allSeries = this.All();
            foreach (var series in allSeries)
            {
                int count = Connection.QueryFirst<int>(@"SELECT COUNT(*) FROM Books WHERE SeriesId = @Id", series, Transaction);

                if (count == 0)
                {
                    this.Remove(series.Id);
                }
            }
        }

        public void CleanSeries(int seriesId)
        {
            int count = Connection.QueryFirst<int>(@"SELECT COUNT(*) FROM Books WHERE SeriesId = @Id", new { Id = seriesId }, Transaction);

            if (count == 0)
            {
                this.Remove(seriesId);
            }
        }

        public BookSeries Find(int id)
        {
            var cacheResult = cache.Find(s => s.Id == id);
            if (cacheResult != null)
                return cacheResult;

            var res = Connection.QueryFirst<BookSeries>("SELECT * FROM Series WHERE Id = @SeriesId LIMIT 1",
                new { SeriesId = id },
                Transaction);
            cache.Add(res);
            return res;
        }

        public BookSeries GetByName(string name)
        {
            var cacheResult = cache.Find(s => s.Name == name);
            if (cacheResult != null)
                return cacheResult;

            return Connection.Query<BookSeries>("SELECT * FROM Series WHERE Name = @Name LIMIT 1",
                new { Name = name },
                Transaction).FirstOrDefault();
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Series WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
            var cacheItem = cache.Find(x => x.Id == id);
            if (cacheItem != null)
            {
                cache.Remove(cacheItem);
            }
        }

        public void Remove(BookSeries entity)
        {
            this.Remove(entity.Id);
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
    }
}