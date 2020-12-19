using System.Collections.Generic;
using Domain;

namespace DataLayer.Interfaces
{
    public interface IQuoteRepository : ICache<Quote>
    {
        void Add(Quote entity);

        IEnumerable<Quote> All();

        Quote Find(int id);

        IEnumerable<Quote> GetQuotesFromBook(int bookId);

        void Remove(int id);

        void Remove(Quote entity);

        void Update(Quote entity);
    }
}
