using System;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IAuthorRepository AuthorRepository { get; }
        IBookRepository BookRepository { get; }
        ICollectionRepository CollectionRepository { get; }
        ICoverRepository CoverRepository { get; }
        IQuoteRepository QuoteRepository { get; }
        ISeriesRepository SeriesRepository { get; }

        void Commit();

        void Truncate();

        void Vacuum();
    }
}
