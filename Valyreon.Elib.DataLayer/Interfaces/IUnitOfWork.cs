using System;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IBookRepository BookRepository { get; }
        ISeriesRepository SeriesRepository { get; }
        ICollectionRepository CollectionRepository { get; }
        IAuthorRepository AuthorRepository { get; }
        ICoverRepository CoverRepository { get; }
        IQuoteRepository QuoteRepository { get; }

        void Truncate();

        void Commit();

        void Vacuum();
    }
}
