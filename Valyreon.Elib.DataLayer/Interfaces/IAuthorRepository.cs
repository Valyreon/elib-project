using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface IAuthorRepository : IRepository<Author>
    {
        Task AddAuthorForBookAsync(Author author, int bookId);
        Task RemoveAuthorForBookAsync(Author author, int bookId);
        Task<IEnumerable<Author>> GetAuthorsOfBookAsync(int bookId);
        Task<Author> GetAuthorWithNameAsync(string name);
        Task<int> CountBooksByAuthorAsync(int authorId);

    }
}
