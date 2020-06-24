using DataLayer.Repositories;
using System;

namespace DataLayer
{
    public interface IUnitOfWork : IDisposable
    {
        IBookRepository BookRepository { get; }
        ISeriesRepository SeriesRepository { get; }
        ICollectionRepository CollectionRepository { get; }
        IAuthorRepository AuthorRepository { get; }
        IEFileRepository EFileRepository { get; }
        IRawFileRepository RawFileRepository { get; }
        ICoverRepository CoverRepository { get; }

        void Truncate();

        void ClearCache();

        void Commit();
    }
}