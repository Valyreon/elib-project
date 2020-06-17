using Dapper;
using Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataLayer.Repositories
{
    public class BookRepository : RepositoryBase, IBookRepository
    {
        private static readonly int pageSize = 30;

        public BookRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public void Add(Book entity)
        {
            entity.Id = Connection.ExecuteScalar<int>(
                @"INSERT INTO Books(Title, SeriesId, IsRead, Cover, WhenRead, NumberInSeries, IsFavorite, PercentageRead, FileId)
                VALUES (@Title, @SeriesId, @IsRead, @Cover, @WhenRead, @NumberInSeries, @IsFavorite, @PercentageRead, @FileId); SELECT last_insert_rowid() ",
                entity,
                Transaction
            );
        }

        public IEnumerable<Book> All()
        {
            return Connection.Query<Book>("SELECT * FROM Books", Transaction).AsList();
        }

        public Book Find(int id)
        {
            return Connection.Query<Book>("SELECT * FROM Books WHERE Id = @BookId LIMIT 1",
                new { BookId = id },
                Transaction).FirstOrDefault();
        }

        public IEnumerable<Book> FindByCollectionId(int collectionId)
        {
            return Connection.Query<Book>("SELECT * FROM CollectionId_Book_View WHERE CollectionId = @CollectionId", new { CollectionId = collectionId }, Transaction);
        }

        public IEnumerable<Book> FindBySeriesId(int seriesId)
        {
            return Connection.Query<Book>("SELECT * FROM Books WHERE SeriesId = @SeriesId", new { SeriesId = seriesId }, Transaction);
        }

        public IEnumerable<Book> FindByAuthorId(int authorId)
        {
            return Connection.Query<Book>("SELECT * FROM AuthorId_Book_View WHERE AuthorId = @AuthorId", new { AuthorId = authorId }, Transaction);
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Books WHERE Id = @RemoveId", new { RemoveId = id });
        }

        public void Remove(Book entity)
        {
            this.Remove(entity.Id);
            entity.Id = 0;
        }

        public void Update(Book entity)
        {
            Connection.Query<Book>(@"UPDATE Books
                                    SET
                                        Title = @Title,
                                        IsRead = @IsRead,
                                        IsFavorite = @IsFavorite,
                                        SeriesId = @SeriesId,
                                        Cover = @Cover,
                                        WhenRead = @WhenRead,
                                        NumberInSeries = @NumberInSeries,
                                        FileId = @FileId,
                                        PercentageRead = @PercentageRead
                                    WHERE Id = @Id", entity, Transaction);
        }

        public IEnumerable<Book> FindPageByFilter(FilterAlt filter, Book lastValueInPage)
        {
            int lastValue = lastValueInPage?.Id ?? -1;
            return GetPage(lastValue);
        }

        public IEnumerable<Book> GetPage(int lastId)
        {
            return Connection.Query<Book>(@"SELECT *
                                            FROM Books
                                            WHERE Id > @LastId
                                            ORDER BY Id
                                            LIMIT @PageSize;", new { LastId = lastId, PageSize = pageSize }, Transaction);
        }
    }
}
