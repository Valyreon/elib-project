using System.Data;
using System.Threading.Tasks;
using Dapper;
using DataLayer.Interfaces;
using Domain;

namespace DataLayer.Repositories
{
    public class RawFileRepository : RepositoryBase, IRawFileRepository
    {
        public RawFileRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public void Add(RawFile entity)
        {
            entity.Id = Connection.ExecuteScalar<int>(
                "INSERT INTO RawFiles(RawContent) VALUES (@RawContent); SELECT last_insert_rowid() ",
                entity,
                Transaction
            );
        }

        public async Task AddAsync(RawFile entity)
        {
            entity.Id = await Connection.ExecuteScalarAsync<int>(
                "INSERT INTO RawFiles(RawContent) VALUES (@RawContent); SELECT last_insert_rowid() ",
                entity,
                Transaction
            );
        }

        public RawFile Find(int id)
        {
            return Connection.QueryFirstOrDefault<RawFile>("SELECT * FROM RawFiles WHERE Id = @FileId LIMIT 1", new { FileId = id }, Transaction);
        }

        public Task<RawFile> FindAsync(int id)
        {
            return Connection.QueryFirstOrDefaultAsync<RawFile>("SELECT * FROM RawFiles WHERE Id = @FileId LIMIT 1", new { FileId = id }, Transaction);
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM RawFiles WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
        }

        public void Remove(RawFile entity)
        {
            Remove(entity.Id);
            entity.Id = 0;
        }

        public async Task RemoveAsync(int id)
        {
            await Connection.ExecuteAsync("DELETE FROM RawFiles WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
        }

        public async Task RemoveAsync(RawFile entity)
        {
            await RemoveAsync(entity.Id);
            entity.Id = 0;
        }
    }
}
