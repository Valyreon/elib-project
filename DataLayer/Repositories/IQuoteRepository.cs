using Domain;
using System.Collections.Generic;

namespace DataLayer.Repositories
{
    public interface IQuoteRepository
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
