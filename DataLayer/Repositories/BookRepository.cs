using Dapper;
using Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataLayer.Repositories
{
    public class BookRepository : RepositoryBase, IBookRepository
    {
        private static readonly List<Book> cache = new List<Book>();

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

            cache.Add(entity);
        }

        public IEnumerable<Book> All()
        {
            var allList = Connection.Query<Book>("SELECT * FROM Books", Transaction).AsList();

            foreach (var series in allList)
            {
                var itemInCache = cache.Find(x => x.Id == series.Id);
                if (itemInCache == null)
                    cache.Add(series);
                else
                    itemInCache.Title = series.Title;
            }

            return cache.ToList();
        }

        public Book Find(int id)
        {
            var cacheResult = cache.Find(s => s.Id == id);
            if (cacheResult != null)
                return cacheResult;

            var res = Connection.QueryFirst<Book>("SELECT * FROM Books WHERE Id = @BookId LIMIT 1",
                new { BookId = id },
                Transaction);
            cache.Add(res);
            return res;
        }

        public IEnumerable<Book> FindByCollectionId(int collectionId)
        {
            List<Book> result = new List<Book>();
            var dbResult = Connection.Query<Book>("SELECT * FROM CollectionId_Book_View WHERE CollectionId = @CollectionId", new { CollectionId = collectionId }, Transaction);

            foreach (var uc in dbResult)
            {
                var inCache = cache.Find(x => x.Id == uc.Id);
                if (inCache == null)
                {
                    cache.Add(uc);
                    result.Add(uc);
                }
                else
                {
                    result.Add(inCache);
                }
            }

            return result;
        }

        public IEnumerable<Book> FindBySeriesId(int seriesId)
        {
            List<Book> result = new List<Book>();
            var dbResult = Connection.Query<Book>("SELECT * FROM Books WHERE SeriesId = @SeriesId", new { SeriesId = seriesId }, Transaction);

            foreach (var uc in dbResult)
            {
                var inCache = cache.Find(x => x.Id == uc.Id);
                if (inCache == null)
                {
                    cache.Add(uc);
                    result.Add(uc);
                }
                else
                {
                    result.Add(inCache);
                }
            }

            return result;
        }

        public IEnumerable<Book> FindByAuthorId(int authorId)
        {
            List<Book> result = new List<Book>();
            var dbResult = Connection.Query<Book>("SELECT * FROM AuthorId_Book_View WHERE AuthorId = @AuthorId", new { AuthorId = authorId }, Transaction);

            foreach (var uc in dbResult)
            {
                var inCache = cache.Find(x => x.Id == uc.Id);
                if (inCache == null)
                {
                    cache.Add(uc);
                    result.Add(uc);
                }
                else
                {
                    result.Add(inCache);
                }
            }

            return result;
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Books WHERE Id = @RemoveId", new { RemoveId = id });
            var cacheItem = cache.Find(x => x.Id == id);
            if (cacheItem != null)
            {
                cache.Remove(cacheItem);
            }
        }

        public void Remove(Book entity)
        {
            this.Remove(entity.Id);
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
            } else if((offset == null && pageSize != null) || (offset != null && pageSize == null))
            {
                throw new ArgumentException("Offset and page size should both be null, or both have values.");
            }

            bool conditionSet = filter.AuthorId.HasValue || filter.CollectionId.HasValue || filter.SeriesId.HasValue ||
                                filter.Read.HasValue || filter.Favorite.HasValue;

            DynamicParameters parameters = new DynamicParameters();

            StringBuilder queryBuilder = new StringBuilder(@$"SELECT MIN(Id) AS Id, Title, SeriesId, IsFavorite, IsRead, WhenRead, CoverId, NumberInSeries, PercentageRead,
                                                            FileId, SeriesName, AuthorName, AuthorId, Tag, CollectionId FROM Full_Join{(conditionSet ? " WHERE (" : " ")}");

            bool conditionsAdded = false;

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
                    queryBuilder.Append($"{(conditionsAdded ? " AND " : "")}IsFavorite = @IsFavorite");
                    parameters.Add("@IsFavorite", filter.Favorite.Value);
                    conditionsAdded = true;
                }

                if (filter.AuthorId.HasValue)
                {
                    parameters.Add("@AuthorId", filter.AuthorId.Value);
                    queryBuilder.Append($"{(conditionsAdded ? " AND " : "")}AuthorId = @AuthorId");
                }
                else if (filter.CollectionId.HasValue)
                {
                    parameters.Add("@CollectionId", filter.CollectionId.Value);
                    queryBuilder.Append($"{(conditionsAdded ? " AND " : "")}CollectionId = @CollectionId");
                }
                else if (filter.SeriesId.HasValue)
                {
                    parameters.Add("@SeriesId", filter.SeriesId.Value);
                    queryBuilder.Append($"{(conditionsAdded ? " AND " : "")}SeriesId = @SeriesId");
                }

                queryBuilder.Append(")");
            }

            if (filter.SearchParameters != null && !string.IsNullOrWhiteSpace(filter.SearchParameters.Token))
            {
                queryBuilder.Append($" {(conditionSet ? " AND " : " WHERE ")} (");
                bool searchAdded = false;
                parameters.Add("@Token", $"%{filter.SearchParameters.Token}%");
                if (filter.SearchParameters.SearchByTitle)
                {
                    queryBuilder.Append($"Title LIKE @Token");
                    searchAdded = true;
                }

                if (filter.SearchParameters.SearchByAuthor)
                {
                    queryBuilder.Append($"{(searchAdded ? " OR " : "")}AuthorName LIKE @Token");
                    searchAdded = true;
                }

                if (filter.SearchParameters.SearchBySeries)
                {
                    queryBuilder.Append($"{(searchAdded ? " OR " : "")}SeriesName LIKE @Token");
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
                queryBuilder.Append($" ORDER BY SeriesName {(filter.Ascending ? " ASC" : " DESC")}, NumberInSeries ASC");
                if (offset != null && pageSize != null)
                    queryBuilder.Append($" LIMIT {offset}, {pageSize}");
            }
            else // title
            {
                queryBuilder.Append(" ORDER BY Title");
            }

            if (!filter.SortBySeries)
            {
                queryBuilder.Append(filter.Ascending ? " ASC" : " DESC");
                if(offset != null && pageSize != null)
                    queryBuilder.Append($" LIMIT {offset}, {pageSize}");
            }

            return new Tuple<string, DynamicParameters>(queryBuilder.ToString(), parameters);
        }

        public IEnumerable<Book> FindPageByFilter(FilterParameters filter, int offset = 0, int pageSize = 25)
        {
            var queryTuple = CreateQueryFromFilter(filter, offset, pageSize);

            var dbResult = Connection.Query<Book>(queryTuple.Item1.ToString(), queryTuple.Item2, Transaction);
            List<Book> result = new List<Book>();

            foreach (var uc in dbResult)
            {
                var inCache = cache.Find(x => x.Id == uc.Id);
                if (inCache == null)
                {
                    cache.Add(uc);
                    result.Add(uc);
                }
                else
                {
                    result.Add(inCache);
                }
            }

            return result;
        }

        private Tuple<string, DynamicParameters> CreateIdQuery(string initial, IEnumerable<int> bookIds)
        {
            DynamicParameters parameters = new DynamicParameters();

            StringBuilder query = new StringBuilder(initial);
            int counter = 0;
            foreach (int id in bookIds)
            {
                string parameterName = $"@Id{counter}";
                if (counter == 0)
                    query.Append($"Id = {parameterName}");
                else
                    query.Append($" OR Id = {parameterName}");
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

            string query = $"SELECT COUNT(*) FROM ({queryTuple.Item1});";

            return Connection.QueryFirst<int>(query, queryTuple.Item2, Transaction);
        }
    }
}
