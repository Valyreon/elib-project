using System.Collections.Generic;
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
    }
}
