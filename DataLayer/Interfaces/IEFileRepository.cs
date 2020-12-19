using System.Collections.Generic;
using Domain;

namespace DataLayer.Interfaces
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
