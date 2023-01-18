using System.Data;
using System.Threading.Tasks;
using Dapper;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Repositories
{
    public class EFileRepository : RepositoryBase<EFile>, IEFileRepository
    {
        public EFileRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public async Task<bool> SignatureExistsAsync(string signature)
        {
            var count = await Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM EBookFiles WHERE Signature = @Signature", new { Signature = signature }, Transaction);
            return count > 0;
        }
    }
}
