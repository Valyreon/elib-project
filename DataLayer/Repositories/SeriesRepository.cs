using Dapper;
using Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataLayer.Repositories
{
    public class SeriesRepository : RepositoryBase, ISeriesRepository
    {
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
        }

        public IEnumerable<BookSeries> All()
        {
            return Connection.Query<BookSeries>("SELECT * FROM Series", Transaction).AsList();
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
            return Connection.Query<BookSeries>("SELECT * FROM Series WHERE Id = @SeriesId LIMIT 1",
                new { SeriesId = id },
                Transaction).FirstOrDefault();
        }

        public BookSeries GetByName(string name)
        {
            return Connection.Query<BookSeries>("SELECT * FROM Series WHERE Name = @Name LIMIT 1",
                new { Name = name },
                Transaction).FirstOrDefault();
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Series WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
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
    }
}
