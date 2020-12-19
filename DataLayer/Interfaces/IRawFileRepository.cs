using Domain;

namespace DataLayer.Interfaces
{
    public interface IRawFileRepository
    {
        void Add(RawFile entity);

        RawFile Find(int id);

        void Remove(int id);

        void Remove(RawFile entity);
    }
}
