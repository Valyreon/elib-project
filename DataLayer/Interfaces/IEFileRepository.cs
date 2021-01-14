using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Interfaces
{
    public interface IEFileRepository
    {
        void Add(EFile entity);

        IEnumerable<EFile> All();

        EFile Find(int id);

        bool SignatureExists(string signature);

        void Remove(int id);

        void Remove(EFile entity);

        Task AddAsync(EFile entity);

        Task<IEnumerable<EFile>> AllAsync();

        Task<EFile> FindAsync(int id);

        Task<bool> SignatureExistsAsync(string signature);

        Task RemoveAsync(int id);

        Task RemoveAsync(EFile entity);
    }
}
