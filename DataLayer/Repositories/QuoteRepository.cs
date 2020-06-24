using Domain;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataLayer.Repositories
{
    public class QuoteRepository : RepositoryBase, IQuoteRepository
    {
        public QuoteRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public void Add(Quote entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Quote> All()
        {
            throw new NotImplementedException();
        }

        public Quote Find(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Quote> GetQuotesFromBook(int bookId)
        {
            throw new NotImplementedException();
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }

        public void Remove(Quote entity)
        {
            throw new NotImplementedException();
        }

        public void Update(Quote entity)
        {
            throw new NotImplementedException();
        }
    }
}