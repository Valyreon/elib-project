using System.Threading.Tasks;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface IUnitOfWorkFactory
    {
        Task<IUnitOfWork> CreateAsync();
    }
}
