using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface IRepository<T> where T: ObservableEntity
    {
        Task<T> Create(T entity);
        Task Create(IEnumerable<T> entities);
        Task<T> Get(int id);
        Task<List<T>> Get(IEnumerable<int> ids, FilterParameters parameters = null);
        Task<List<T>> GetAll(FilterParameters parameters = null);
        Task<T> Update(T entity);
        Task Update(IEnumerable<T> entities);
        Task Delete(T entity);
        Task Delete(IEnumerable<T> entities);
    }
}
