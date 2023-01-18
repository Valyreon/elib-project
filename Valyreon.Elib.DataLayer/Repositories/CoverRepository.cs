using System.Data;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Repositories
{
    public class CoverRepository : RepositoryBase<Cover>, ICoverRepository
    {
        public CoverRepository(IDbTransaction transaction) : base(transaction)
        {
        }
    }
}
