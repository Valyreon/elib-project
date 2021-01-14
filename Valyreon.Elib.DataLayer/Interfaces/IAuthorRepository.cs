using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface IAuthorRepository : ICache<Author>
    {
        void Add(Author entity);

        void AddAuthorForBook(Author author, int bookId);

        void RemoveAuthorForBook(Author author, int bookId);

        IEnumerable<Author> All();

        Author Find(int id);

        IEnumerable<Author> GetAuthorsOfBook(int bookId);

        Author GetAuthorWithName(string name);

        int CountBooksByAuthor(int authorId);

        void Remove(int id);

        void Remove(Author entity);

        void Update(Author entity);

        Task AddAsync(Author entity);

        Task AddAuthorForBookAsync(Author author, int bookId);

        Task RemoveAuthorForBookAsync(Author author, int bookId);

        Task<IEnumerable<Author>> AllAsync();

        Task<Author> FindAsync(int id);

        Task<IEnumerable<Author>> GetAuthorsOfBookAsync(int bookId);

        Task<Author> GetAuthorWithNameAsync(string name);

        Task<int> CountBooksByAuthorAsync(int authorId);

        Task RemoveAsync(int id);

        Task RemoveAsync(Author entity);

        Task UpdateAsync(Author entity);
    }
}
