using Domain;
using System.Collections.Generic;

namespace DataLayer.Repositories
{
    public interface IEFileRepository
    {
        void Add(EFile entity);

        IEnumerable<EFile> All();

        EFile Find(int id);

        bool SignatureExists(string signature);

        void Remove(int id);

        void Remove(EFile entity);
    }
}