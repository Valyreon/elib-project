using Domain;
using System.Collections.Generic;

namespace DataLayer.Repositories
{
    public interface ISeriesRepository
    {
        void Add(BookSeries entity);
        IEnumerable<BookSeries> All();
        BookSeries Find(int id);
        void Remove(int id);
        void Remove(BookSeries entity);
        void Update(BookSeries entity);
        void CleanSeries();
        void CleanSeries(int seriesId);
    }
}
