using System.Threading.Tasks;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface ICoverRepository
    {
        void Add(Cover entity);

        void Update(Cover entity);

        Cover Find(int id);

        void Remove(int id);

        void Remove(Cover entity);

        Task AddAsync(Cover entity);

        Task UpdateAsync(Cover entity);

        Task<Cover> FindAsync(int id);

        Task RemoveAsync(int id);

        Task RemoveAsync(Cover entity);
    }
}
