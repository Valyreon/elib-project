﻿using Dapper;
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

        public IEnumerable<Book> FindPageByFilter(FilterParameters filter, int offset = 0, int pageSize = 25)
        {
            if (filter == null)
            {
                throw new ArgumentException("Filter is null");
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

            if(filter.SearchParameters != null && !string.IsNullOrWhiteSpace(filter.SearchParameters.Token))
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
                queryBuilder.Append(" ORDER BY SeriesName");
            }
            else // title
            {
                queryBuilder.Append(" ORDER BY Title");
            }

            queryBuilder.Append($"{(filter.Ascending ? " ASC" : " DESC")} LIMIT {offset}, {pageSize}");
            var dbResult = Connection.Query<Book>(queryBuilder.ToString(), parameters, Transaction);
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
    }
}
