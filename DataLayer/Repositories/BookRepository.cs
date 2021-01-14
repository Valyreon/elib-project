using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DataLayer.Extensions;
using DataLayer.Interfaces;
using Domain;

namespace DataLayer.Repositories
{
    public class BookRepository : RepositoryBase, IBookRepository
    {
        private static readonly IDictionary<int, Book> cache = new Dictionary<int, Book>();

        public BookRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public void Add(Book entity)
        {
            entity.Id = Connection.ExecuteScalar<int>(
                @"INSERT INTO Books(Title, SeriesId, IsRead, CoverId, WhenRead, NumberInSeries, IsFavorite, PercentageRead, FileId)
                VALUES (@Title, @SeriesId, @IsRead, @CoverId, @WhenRead, @NumberInSeries, @IsFavorite, @PercentageRead, @FileId); SELECT last_insert_rowid() ",
                entity,
                Transaction
            );

            cache.Add(entity.Id, entity);
        }

        public IEnumerable<Book> All()
        {
            var allList = Connection.Query<Book>("SELECT * FROM Books", Transaction);

            foreach (var book in allList.Where(x => !cache.ContainsKey(x.Id)))
            {
                cache.Add(book.Id, book);
            }

            return cache.Values.ToList();
        }

        public Book Find(int id)
        {
            if (cache.TryGetValue(id, out var bookFromCache))
            {
                return bookFromCache;
            }

            var res = Connection.QueryFirst<Book>("SELECT * FROM Books WHERE Id = @BookId LIMIT 1",
                new { BookId = id },
                Transaction);

            if (res != null)
            {
                cache.Add(res.Id, res);
            }

            return res;
        }

        public IEnumerable<Book> FindByCollectionId(int collectionId)
        {
            var dbResult = Connection.Query<Book>(
                "SELECT * FROM CollectionId_Book_View WHERE CollectionId = @CollectionId",
                new { CollectionId = collectionId },
                Transaction);

            return cache.FilterAndUpdateCache(dbResult);
        }

        public IEnumerable<Book> FindBySeriesId(int seriesId)
        {
            var dbResult = Connection.Query<Book>("SELECT * FROM Books WHERE SeriesId = @SeriesId", new { SeriesId = seriesId }, Transaction);
            return cache.FilterAndUpdateCache(dbResult);
        }

        public IEnumerable<Book> FindByAuthorId(int authorId)
        {
            var dbResult = Connection.Query<Book>("SELECT * FROM AuthorId_Book_View WHERE AuthorId = @AuthorId", new { AuthorId = authorId }, Transaction);
            return cache.FilterAndUpdateCache(dbResult);
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Books WHERE Id = @RemoveId", new { RemoveId = id });
            cache.Remove(id);
        }

        public void Remove(Book entity)
        {
            Remove(entity.Id);
            entity.Id = 0;
        }

        public void Update(Book entity)
        {
            Connection.Execute(@"UPDATE Books
                                    SET
                                        Title = @Title,
                                        IsRead = @IsRead,
                                        IsFavorite = @IsFavorite,
                                        SeriesId = @SeriesId,
                                        CoverId = @CoverId,
                                        WhenRead = @WhenRead,
                                        NumberInSeries = @NumberInSeries,
                                        FileId = @FileId,
                                        PercentageRead = @PercentageRead
                                    WHERE Id = @Id", entity, Transaction);
        }

        private Tuple<string, DynamicParameters> CreateQueryFromFilter(FilterParameters filter, int? offset, int? pageSize)
        {
            if (filter == null)
            {
                throw new ArgumentException("Filter is null");
            }
            else if ((offset == null && pageSize != null) || (offset != null && pageSize == null))
            {
                throw new ArgumentException("Offset and page size should both be null, or both have values.");
            }

            var conditionSet = filter.AuthorId.HasValue || filter.CollectionId.HasValue || filter.SeriesId.HasValue ||
                                filter.Read.HasValue || filter.Favorite.HasValue;

            var parameters = new DynamicParameters();

            var queryBuilder = new StringBuilder(@$"SELECT MIN(Id) AS Id, Title, SeriesId, IsFavorite, IsRead, WhenRead, CoverId, NumberInSeries, PercentageRead,
                                                            FileId, SeriesName, AuthorName, AuthorId, Tag, CollectionId FROM Full_Join{(conditionSet ? " WHERE (" : " ")}");

            var conditionsAdded = false;

            if (conditionSet)
            {
                if (filter.Read.HasValue)
                {
                    queryBuilder.Append("IsRead = @IsRead");
                    parameters.Add("@IsRead", filter.Read.Value);
                    conditionsAdded = true;
                }

                if (filter.Favorite.HasValue)
                {
                    queryBuilder.Append(conditionsAdded ? " AND " : "").Append("IsFavorite = @IsFavorite");
                    parameters.Add("@IsFavorite", filter.Favorite.Value);
                    conditionsAdded = true;
                }

                if (filter.AuthorId.HasValue)
                {
                    parameters.Add("@AuthorId", filter.AuthorId.Value);
                    queryBuilder.Append(conditionsAdded ? " AND " : "").Append("AuthorId = @AuthorId");
                }
                else if (filter.CollectionId.HasValue)
                {
                    parameters.Add("@CollectionId", filter.CollectionId.Value);
                    queryBuilder.Append(conditionsAdded ? " AND " : "").Append("CollectionId = @CollectionId");
                }
                else if (filter.SeriesId.HasValue)
                {
                    parameters.Add("@SeriesId", filter.SeriesId.Value);
                    queryBuilder.Append(conditionsAdded ? " AND " : "").Append("SeriesId = @SeriesId");
                }

                queryBuilder.Append(")");
            }

            if (filter.SearchParameters != null && !string.IsNullOrWhiteSpace(filter.SearchParameters.Token))
            {
                queryBuilder.Append(' ').Append(conditionSet ? " AND " : " WHERE ").Append(" (");
                var searchAdded = false;
                parameters.Add("@Token", $"%{filter.SearchParameters.Token}%");
                if (filter.SearchParameters.SearchByTitle)
                {
                    queryBuilder.Append("Title LIKE @Token");
                    searchAdded = true;
                }

                if (filter.SearchParameters.SearchByAuthor)
                {
                    queryBuilder.Append(searchAdded ? " OR " : "").Append("AuthorName LIKE @Token");
                    searchAdded = true;
                }

                if (filter.SearchParameters.SearchBySeries)
                {
                    queryBuilder.Append(searchAdded ? " OR " : "").Append("SeriesName LIKE @Token");
                }
                queryBuilder.Append(")");
            }

            queryBuilder.Append(" GROUP BY Id");

            if (filter.SortByImportOrder)
            {
                queryBuilder.Append(" ORDER BY Id");
            }
            else if (filter.SortByAuthor)
            {
                queryBuilder.Append(" ORDER BY AuthorName");
            }
            else if (filter.SortBySeries)
            {
                queryBuilder.Append(" ORDER BY SeriesName ").Append(filter.Ascending ? " ASC" : " DESC").Append(", NumberInSeries ASC");
                if (offset != null && pageSize != null)
                {
                    queryBuilder.Append(" LIMIT ").Append(offset).Append(", ").Append(pageSize);
                }
            }
            else // title
            {
                queryBuilder.Append(" ORDER BY Title");
            }

            if (!filter.SortBySeries)
            {
                queryBuilder.Append(filter.Ascending ? " ASC" : " DESC");
                if (offset != null && pageSize != null)
                {
                    queryBuilder.Append(" LIMIT ").Append(offset).Append(", ").Append(pageSize);
                }
            }

            return new Tuple<string, DynamicParameters>(queryBuilder.ToString(), parameters);
        }

        public IEnumerable<Book> FindPageByFilter(FilterParameters filter, int offset = 0, int pageSize = 25)
        {
            var queryTuple = CreateQueryFromFilter(filter, offset, pageSize);
            var dbResult = Connection.Query<Book>(queryTuple.Item1, queryTuple.Item2, Transaction);
            return cache.FilterAndUpdateCache(dbResult);
        }

        private Tuple<string, DynamicParameters> CreateIdQuery(string initial, IEnumerable<int> bookIds)
        {
            var parameters = new DynamicParameters();

            var query = new StringBuilder(initial);
            var counter = 0;
            foreach (var id in bookIds)
            {
                var parameterName = $"@Id{counter}";
                if (counter == 0)
                {
                    query.Append("Id = ").Append(parameterName);
                }
                else
                {
                    query.Append(" OR Id = ").Append(parameterName);
                }

                counter++;
                parameters.Add(parameterName, id);
            }

            return new Tuple<string, DynamicParameters>(query.ToString(), parameters);
        }

        public IEnumerable<Book> GetBooks(IEnumerable<int> Ids)
        {
            var x = CreateIdQuery("SELECT * FROM Books WHERE ", Ids);

            return Connection.Query<Book>(x.Item1, x.Item2, Transaction);
        }

        public void ClearCache()
        {
            cache.Clear();
        }

        public int Count(FilterParameters filter)
        {
            var queryTuple = CreateQueryFromFilter(filter, null, null);

            var query = $"SELECT COUNT(*) FROM ({queryTuple.Item1});";

            return Connection.QueryFirst<int>(query, queryTuple.Item2, Transaction);
        }

        public IEnumerable<Book> GetCachedObjects()
        {
            return cache.Values.ToList();
        }

        public async Task AddAsync(Book entity)
        {
            entity.Id = await Connection.ExecuteScalarAsync<int>(
                @"INSERT INTO Books(Title, SeriesId, IsRead, CoverId, WhenRead, NumberInSeries, IsFavorite, PercentageRead, FileId)
                VALUES (@Title, @SeriesId, @IsRead, @CoverId, @WhenRead, @NumberInSeries, @IsFavorite, @PercentageRead, @FileId); SELECT last_insert_rowid() ",
                entity,
                Transaction
            );

            cache.Add(entity.Id, entity);
        }

        public async Task<IEnumerable<Book>> AllAsync()
        {
            var allList = await Connection.QueryAsync<Book>("SELECT * FROM Books", Transaction);

            foreach (var book in allList.Where(x => !cache.ContainsKey(x.Id)))
            {
                cache.Add(book.Id, book);
            }

            return cache.Values.ToList();
        }

        public async Task<Book> FindAsync(int id)
        {
            if (cache.TryGetValue(id, out var bookFromCache))
            {
                return bookFromCache;
            }

            var res = await Connection.QueryFirstAsync<Book>("SELECT * FROM Books WHERE Id = @BookId LIMIT 1",
                new { BookId = id },
                Transaction);

            if (res != null)
            {
                cache.Add(res.Id, res);
            }

            return res;
        }

        public async Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<int> Ids)
        {
            var x = CreateIdQuery("SELECT * FROM Books WHERE ", Ids);

            return await Connection.QueryAsync<Book>(x.Item1, x.Item2, Transaction);
        }

        public async Task<IEnumerable<Book>> FindBySeriesIdAsync(int seriesId)
        {
            var dbResult = await Connection.QueryAsync<Book>("SELECT * FROM Books WHERE SeriesId = @SeriesId", new { SeriesId = seriesId }, Transaction);
            return cache.FilterAndUpdateCache(dbResult);
        }

        public async Task<IEnumerable<Book>> FindByCollectionIdAsync(int collectionId)
        {
            var dbResult = await Connection.QueryAsync<Book>(
                "SELECT * FROM CollectionId_Book_View WHERE CollectionId = @CollectionId",
                new { CollectionId = collectionId },
                Transaction);

            return cache.FilterAndUpdateCache(dbResult);
        }

        public async Task<IEnumerable<Book>> FindByAuthorIdAsync(int authorId)
        {
            var dbResult = await Connection.QueryAsync<Book>("SELECT * FROM AuthorId_Book_View WHERE AuthorId = @AuthorId", new { AuthorId = authorId }, Transaction);
            return cache.FilterAndUpdateCache(dbResult);
        }

        public async Task<IEnumerable<Book>> FindPageByFilterAsync(FilterParameters filter, int offset, int pageSize)
        {
            var queryTuple = await Task.Run(() => CreateQueryFromFilter(filter, offset, pageSize));
            var dbResult = await Connection.QueryAsync<Book>(queryTuple.Item1, queryTuple.Item2, Transaction);
            return cache.FilterAndUpdateCache(dbResult);
        }

        public async Task<int> CountAsync(FilterParameters filter)
        {
            var queryTuple = CreateQueryFromFilter(filter, null, null);

            var query = $"SELECT COUNT(*) FROM ({queryTuple.Item1});";

            return await Connection.QueryFirstAsync<int>(query, queryTuple.Item2, Transaction);
        }

        public async Task RemoveAsync(int id)
        {
            await Connection.ExecuteAsync("DELETE FROM Books WHERE Id = @RemoveId", new { RemoveId = id });
            cache.Remove(id);
        }

        public async Task RemoveAsync(Book entity)
        {
            await RemoveAsync(entity.Id);
            entity.Id = 0;
        }

        public async Task UpdateAsync(Book entity)
        {
            await Connection.ExecuteAsync(@"UPDATE Books
                                    SET
                                        Title = @Title,
                                        IsRead = @IsRead,
                                        IsFavorite = @IsFavorite,
                                        SeriesId = @SeriesId,
                                        CoverId = @CoverId,
                                        WhenRead = @WhenRead,
                                        NumberInSeries = @NumberInSeries,
                                        FileId = @FileId,
                                        PercentageRead = @PercentageRead
                                    WHERE Id = @Id", entity, Transaction);
        }
    }
}
