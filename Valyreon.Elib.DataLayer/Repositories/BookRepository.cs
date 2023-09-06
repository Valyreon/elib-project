using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Valyreon.Elib.DataLayer.Extensions;
using Valyreon.Elib.DataLayer.Filters;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Repositories
{
    public class BookRepository : RepositoryBase<Book>, IBookRepository
    {
        public BookRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        private static Tuple<string, DynamicParameters> CreateQueryFromFilter(BookFilter filter, int? offset = null, int? pageSize = null, bool onlyIds = false)
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

            var queryBuilder = new StringBuilder(@$"SELECT MIN(Id) AS Id{(onlyIds ? string.Empty : ", *")} FROM Full_Join {(conditionSet ? " WHERE (" : " ")}");

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

                queryBuilder.Append(')');
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                queryBuilder.Append(' ').Append(conditionSet ? " AND " : " WHERE ").Append(" (");
                parameters.Add("@Token", $"%{filter.SearchText}%");
                queryBuilder.Append("Title LIKE @Token OR AuthorName LIKE @Token OR SeriesName LIKE @Token");
                queryBuilder.Append(')');
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

        public int Count(BookFilter filter)
        {
            var queryTuple = CreateQueryFromFilter(filter, null, null);

            var query = $"SELECT COUNT(*) FROM ({queryTuple.Item1});";

            return Connection.QueryFirst<int>(query, queryTuple.Item2, Transaction);
        }

        public async Task<IEnumerable<Book>> FindBySeriesIdAsync(int seriesId)
        {
            var result = await Connection.QueryAsync<Book>("SELECT * FROM Books WHERE SeriesId = @SeriesId", new { SeriesId = seriesId }, Transaction);

            return Cache.FilterAndUpdateCache(result);
        }

        public async Task<IEnumerable<Book>> FindByCollectionIdAsync(int collectionId)
        {
            var result = await Connection.QueryAsync<Book>(
                "SELECT * FROM CollectionId_Book_View WHERE CollectionId = @CollectionId",
                new { CollectionId = collectionId },
                Transaction);

            return Cache.FilterAndUpdateCache(result);
        }

        public async Task<IEnumerable<Book>> FindByAuthorIdAsync(int authorId)
        {
            var result = await Connection.QueryAsync<Book>("SELECT * FROM AuthorId_Book_View WHERE AuthorId = @AuthorId", new { AuthorId = authorId }, Transaction);
            return Cache.FilterAndUpdateCache(result);
        }

        public async Task<IEnumerable<Book>> GetByFilterAsync(BookFilter filter, int? offset = null, int? pageSize = null)
        {
            var queryTuple = CreateQueryFromFilter(filter, offset, pageSize);
            var result = await Connection.QueryAsync<Book>(queryTuple.Item1, queryTuple.Item2, Transaction);

            return Cache.FilterAndUpdateCache(result);
        }

        public Task<IEnumerable<int>> GetIdsByFilterAsync(BookFilter filter)
        {
            var queryTuple = CreateQueryFromFilter(filter, null, null);
            return Connection.QueryAsync<int>(queryTuple.Item1, queryTuple.Item2, Transaction);
        }

        public async Task<int> CountAsync(BookFilter filter)
        {
            var queryTuple = CreateQueryFromFilter(filter, null, null);

            var query = $"SELECT COUNT(*) FROM ({queryTuple.Item1});";

            return await Connection.QueryFirstAsync<int>(query, queryTuple.Item2, Transaction);
        }

        public async Task<bool> SignatureExistsAsync(string signature)
        {
            var count = await Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM Books WHERE Signature = @Signature", new { Signature = signature }, Transaction);
            return count > 0;
        }

        public async Task<bool> PathExistsAsync(string path)
        {
            var count = await Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM Books WHERE Path = @Path", new { Path = path }, Transaction);
            return count > 0;
        }

        public async Task<Book> GetByPathAsync(string path)
        {
            var result = await Connection.QuerySingleOrDefaultAsync<Book>("SELECT * FROM Books WHERE Path = @Path", new { Path = path }, Transaction);
            return Cache.FilterAndUpdateCache(result);
        }

        public async Task<Book> GetBySignatureAsync(string signature)
        {
            var result = await Connection.QuerySingleOrDefaultAsync<Book>("SELECT * FROM Books WHERE Signature = @Signature", new { Signature = signature }, Transaction);
            return Cache.FilterAndUpdateCache(result);
        }
    }
}
