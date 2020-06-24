using Dapper;
using Domain;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataLayer.Repositories
{
    public class EFileRepository : RepositoryBase, IEFileRepository
    {
        public EFileRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public void Add(EFile entity)
        {
            if (entity.RawFileId == 0)
                throw new ArgumentException("EFile must have the raw file already added.");

            entity.Id = Connection.ExecuteScalar<int>(
                "INSERT INTO EBookFiles(Format, Signature, RawFileId) VALUES (@Format, @Signature, @RawFileId); SELECT last_insert_rowid() ",
                entity,
                Transaction
            );
        }

        public IEnumerable<EFile> All()
        {
            return Connection.Query<EFile>("SELECT * FROM EBookFiles", Transaction).AsList();
        }

        public EFile Find(int id)
        {
            return Connection.QueryFirst<EFile>("SELECT * FROM EBookFiles WHERE Id = @FileId LIMIT 1", new { FileId = id }, Transaction);
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Series WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
        }

        public void Remove(EFile entity)
        {
            this.Remove(entity.Id);
            entity.Id = 0;
        }

        public bool SignatureExists(string signature)
        {
            int count = Connection.QueryFirst<int>("SELECT COUNT(*) FROM EBookFiles WHERE Signature = @Signature LIMIT 1", new { Signature = signature }, Transaction);
            return count > 0;
        }
    }
}