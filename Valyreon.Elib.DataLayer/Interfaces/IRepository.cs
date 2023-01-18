using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces;

public interface IRepository<T> where T: ObservableEntity
{
    string Table { get; }
    IEnumerable<string> ColumnNames { get; }

    Task CreateAsync(T entity);
    Task CreateAsync(IEnumerable<T> entities);
    Task<T> FindAsync(int id);
    Task<IEnumerable<T>> FindAsync(IEnumerable<int> ids, QueryParameters parameters = null);
    Task<IEnumerable<T>> GetAllAsync(QueryParameters parameters = null);
    Task UpdateAsync(T entity);
    Task UpdateAsync(IEnumerable<T> entities);
    Task DeleteAsync(T entity);
    Task DeleteAsync(int id);
    Task DeleteAsync(IEnumerable<T> entities);
    Task DeleteAsync(IEnumerable<int> ids);
}
