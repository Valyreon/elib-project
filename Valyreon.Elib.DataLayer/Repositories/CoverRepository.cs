using System.Data;
using System.Threading.Tasks;
using Dapper;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Repositories
{
    public class CoverRepository : RepositoryBase, ICoverRepository
    {
        public CoverRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public void Add(Cover entity)
        {
            entity.Id = Connection.ExecuteScalar<int>("INSERT INTO Covers(Image) VALUES (@Image); SELECT last_insert_rowid() ", entity, Transaction);
        }

        public async Task AddAsync(Cover entity)
        {
            entity.Id = await Connection.ExecuteScalarAsync<int>("INSERT INTO Covers(Image) VALUES (@Image); SELECT last_insert_rowid() ", entity, Transaction);
        }

        public Cover Find(int id)
        {
            return Connection.QueryFirstOrDefault<Cover>("SELECT * FROM Covers WHERE Id = @CoverId LIMIT 1", new { CoverId = id }, Transaction);
        }

        public async Task<Cover> FindAsync(int id)
        {
            return await Connection.QueryFirstOrDefaultAsync<Cover>("SELECT * FROM Covers WHERE Id = @CoverId LIMIT 1", new { CoverId = id }, Transaction);
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Covers WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
        }

        public void Remove(Cover entity)
        {
            Remove(entity.Id);
            entity.Id = 0;
        }

        public async Task RemoveAsync(int id)
        {
            await Connection.ExecuteAsync("DELETE FROM Covers WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
        }

        public async Task RemoveAsync(Cover entity)
        {
            await RemoveAsync(entity.Id);
            entity.Id = 0;
        }

        public void Update(Cover entity)
        {
            Connection.Execute("UPDATE Covers SET Image = @Image WHERE Id = @Id", entity, Transaction);
        }

        public async Task UpdateAsync(Cover entity)
        {
            await Connection.ExecuteAsync("UPDATE Covers SET Image = @Image WHERE Id = @Id", entity, Transaction);
        }
    }
}
