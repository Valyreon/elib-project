using Dapper;
using Domain;
using System;
using System.Data;
using System.Linq;

namespace DataLayer.Repositories
{
    public class CoverRepository : RepositoryBase, ICoverRepository
    {
        public CoverRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public void Add(Cover entity)
        {
            entity.Id = Connection.ExecuteScalar<int>(
                "INSERT INTO Covers(Image) VALUES (@Image); SELECT last_insert_rowid() ",
                entity,
                Transaction
            );
        }

        public Cover Find(int id)
        {
            return Connection.Query<Cover>("SELECT * FROM Covers WHERE Id = @CoverId LIMIT 1", new { CoverId = id }, Transaction).FirstOrDefault();
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Covers WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
        }

        public void Remove(Cover entity)
        {
            this.Remove(entity.Id);
            entity.Id = 0;
        }

        public void Update(Cover entity)
        {
            Connection.Execute(@"UPDATE Covers
                                    SET
                                        Image = @Image
                                    WHERE Id = @Id", entity, Transaction);
        }
    }
}
