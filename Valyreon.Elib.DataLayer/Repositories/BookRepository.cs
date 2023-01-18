using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Valyreon.Elib.DataLayer.Extensions;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Repositories
{
    public class BookRepository : RepositoryBase<Book>, IBookRepository
    {
        public BookRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        private static Tuple<string, DynamicParameters> CreateQueryFromFilter(FilterParameters filter, int? offset, int? pageSize)
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

            var queryBuilder = new StringBuilder(@$"SELECT MIN(Id) AS Id, Title, SeriesId, IsFavorite, IsRead, WhenRead, CoverId, NumberInSeries, 
                                                            FileId, SeriesName, AuthorName, AuthorId, Tag, CollectionId FROM Full_Join {(conditionSet ? " WHERE (" : " ")}");

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

        public int Count(FilterParameters filter)
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

        public async Task<IEnumerable<Book>> FindPageByFilterAsync(FilterParameters filter, int offset, int pageSize)
        {
            var queryTuple = CreateQueryFromFilter(filter, offset, pageSize);
            var result = await Connection.QueryAsync<Book>(queryTuple.Item1, queryTuple.Item2, Transaction);

            return Cache.FilterAndUpdateCache(result);
        }

        public async Task<int> CountAsync(FilterParameters filter)
        {
            var queryTuple = CreateQueryFromFilter(filter, null, null);

            var query = $"SELECT COUNT(*) FROM ({queryTuple.Item1});";

            return await Connection.QueryFirstAsync<int>(query, queryTuple.Item2, Transaction);
        }
    }
}
