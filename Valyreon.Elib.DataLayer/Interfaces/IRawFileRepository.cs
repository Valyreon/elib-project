using System.Threading.Tasks;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface IRawFileRepository
    {
        void Add(RawFile entity);

        RawFile Find(int id);

        void Remove(int id);

        void Remove(RawFile entity);

        Task AddAsync(RawFile entity);

        Task<RawFile> FindAsync(int id);

        Task RemoveAsync(int id);

        Task RemoveAsync(RawFile entity);
    }
}
