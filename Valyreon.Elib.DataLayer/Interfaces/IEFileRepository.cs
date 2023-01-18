using System.Threading.Tasks;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface IEFileRepository : IRepository<EFile>
    {
        Task<bool> SignatureExistsAsync(string signature);
    }
}
