using System.Data;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Repositories
{
    public class RawFileRepository : RepositoryBase<RawFile>, IRawFileRepository
    {
        public RawFileRepository(IDbTransaction transaction) : base(transaction)
        {
        }
    }
}
