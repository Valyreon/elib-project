using Dapper;
using Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DataLayer.Repositories
{
    public class BookRepository : RepositoryBase, IBookRepository
    {
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
                                        CoverId = @CoverId,
                                        WhenRead = @WhenRead,
                                        NumberInSeries = @NumberInSeries,
                                        FileId = @FileId,
                                        PercentageRead = @PercentageRead
                                    WHERE Id = @Id", entity, Transaction);
        }

        public IEnumerable<Book> FindPageByFilter(Filter filter, int offset = 0, int pageSize = 25)
        {
            if (filter == null)
            {
                return GetPage(offset);
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

            if(!string.IsNullOrWhiteSpace(filter.Token))
            {
                queryBuilder.Append($" {(conditionSet ? " AND " : " WHERE ")} (");
                bool searchAdded = false;
                parameters.Add("@Token", $"%{filter.Token}%");
                if (filter.SearchByName)
                {
                    queryBuilder.Append($"Title LIKE @Token");
                    searchAdded = true;
                }

                if (filter.SearchByAuthor)
                {
                    queryBuilder.Append($"{(searchAdded ? " OR " : "")}AuthorName LIKE @Token");
                    searchAdded = true;
                }

                if (filter.SearchBySeries)
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
            string query = queryBuilder.ToString();
            var ret = Connection.Query<Book>(query, parameters, Transaction);
            return ret;

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

        public IEnumerable<Book> GetPage(int lastId, int pageSize = 25)
        {
            return Connection.Query<Book>(@"SELECT *
                                            FROM Books
                                            WHERE Id > @LastId
                                            ORDER BY Id
                                            LIMIT @PageSize;", new { LastId = lastId, PageSize = pageSize }, Transaction);
        }

        public IEnumerable<Book> GetBooks(IEnumerable<int> Ids)
        {
            var x = CreateIdQuery("SELECT * FROM Books WHERE ", Ids);

            return Connection.Query<Book>(x.Item1, x.Item2, Transaction);
        }
    }
}
