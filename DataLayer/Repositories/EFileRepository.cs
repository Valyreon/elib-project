using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using DataLayer.Interfaces;
using Domain;

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
            {
                throw new ArgumentException("EFile must have the raw file already added.");
            }

            entity.Id = Connection.ExecuteScalar<int>(
                "INSERT INTO EBookFiles(Format, Signature, RawFileId) VALUES (@Format, @Signature, @RawFileId); SELECT last_insert_rowid() ",
                entity,
                Transaction
            );
        }

        public IEnumerable<EFile> All()
        {
            return Connection.Query<EFile>("SELECT * FROM EBookFiles", Transaction);
        }

        public EFile Find(int id)
        {
            return Connection.Query<EFile>("SELECT * FROM EBookFiles WHERE Id = @FileId LIMIT 1", new { FileId = id }, Transaction).FirstOrDefault();
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Series WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
        }

        public void Remove(EFile entity)
        {
            Remove(entity.Id);
            entity.Id = 0;
        }

        public bool SignatureExists(string signature)
        {
            var count = Connection.QueryFirst<int>("SELECT COUNT(*) FROM EBookFiles WHERE Signature = @Signature LIMIT 1", new { Signature = signature }, Transaction);
            return count > 0;
        }
    }
}