using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Valyreon.Elib.DataLayer.Extensions;
using Valyreon.Elib.DataLayer.Filters;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Repositories
{
    public class AuthorRepository : RepositoryBase<Author>, IAuthorRepository
    {
        public AuthorRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public async Task AddAuthorForBookAsync(Author author, int bookId)
        {
            if (author.Id == 0)
            {
                await CreateAsync(author);
            }

            await Connection.ExecuteAsync("INSERT INTO AuthorBooks(AuthorId, BookId) VALUES (@AuthorId, @BookId)", new { AuthorId = author.Id, BookId = bookId }, Transaction);
        }

        public async Task<int> CountBooksByAuthorAsync(int authorId)
        {
            return await Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM AuthorBooks WHERE AuthorId = @AuthorId", new { AuthorId = authorId }, Transaction);
        }

        public async Task<IEnumerable<Author>> GetAuthorsOfBookAsync(int bookId)
        {
            var dbResult = await Connection.QueryAsync<Author>("SELECT Id, Name FROM BookId_Author_View WHERE BookId = @BookId", new { BookId = bookId }, Transaction);
            return Cache.FilterAndUpdateCache(dbResult);
        }

        public async Task<Author> GetAuthorWithNameAsync(string name)
        {
            var result = Cache.Values.FirstOrDefault(x => x.Name == name);

            return result ?? await Connection.QueryFirstOrDefaultAsync<Author>("SELECT * FROM Authors WHERE Name = @AuthorName LIMIT 1", new { AuthorName = name }, Transaction);
        }

        public async Task<IEnumerable<Author>> SearchAsync(string token)
        {
            var result = await Connection.QueryAsync<Author>("SELECT * FROM Authors WHERE Name LIKE @Token", new { Token = $"%{token}%" }, Transaction);
            return Cache.FilterAndUpdateCache(result);
        }

        public async Task RemoveAuthorForBookAsync(Author author, int bookId)
        {
            await Connection.ExecuteAsync("DELETE FROM AuthorBooks WHERE AuthorId = @AuthorId AND BookId = @BookId", new { AuthorId = author.Id, BookId = bookId }, Transaction);
            Cache.Remove(author.Id);
        }

        public async Task<IEnumerable<Author>> GetAuthorsWithNumberOfBooks(Filter filter)
        {
            string query;
            if (filter.CollectionId == null)
            {
                query = "SELECT * FROM Authors_BookCount_View";

                if (!string.IsNullOrWhiteSpace(filter.SearchText))
                {
                    query += $" WHERE Name LIKE '%{filter.SearchText}%'";
                }
            }
            else
            {
                query = "SELECT * FROM Authors_BookCount_Collections_View WHERE CollectionId = @CollectionId";

                if (!string.IsNullOrWhiteSpace(filter.SearchText))
                {
                    query += $" AND Name LIKE '%{filter.SearchText}%'";
                }
            }

            query += $" ORDER BY {(filter.SortByBookCount ? "BookCount" : "Name")} {(filter.Ascending ? "ASC" : "DESC")}";

            var result = await Connection.QueryAsync<dynamic>(query, new { filter.CollectionId }, Transaction);
            return ProcessWithBookCount(result);
        }

        private static IEnumerable<Author> ProcessWithBookCount(IEnumerable<dynamic> dynamicResult)
        {
            foreach (var row in dynamicResult)
            {
                var fields = row as IDictionary<string, object>;

                yield return new Author
                {
                    Id = (int)(long)fields["Id"],
                    Name = (string)fields["Name"],
                    NumberOfBooks = (int)(long)fields["BookCount"]
                };
            }
        }
    }
}
