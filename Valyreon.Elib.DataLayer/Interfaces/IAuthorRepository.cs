using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer.Filters;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface IAuthorRepository : IRepository<Author>
    {
        Task AddAuthorForBookAsync(Author author, int bookId);

        Task<int> CountBooksByAuthorAsync(int authorId);

        Task<IEnumerable<Author>> GetAuthorsOfBookAsync(int bookId);

        Task<IEnumerable<Author>> GetAuthorsWithNumberOfBooks(Filter filter);

        Task<Author> GetAuthorWithNameAsync(string name);

        Task RemoveAuthorForBookAsync(Author author, int bookId);

        Task<IEnumerable<Author>> SearchAsync(string token);
    }
}
