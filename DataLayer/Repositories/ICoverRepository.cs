using Domain;

namespace DataLayer.Repositories
{
    public interface ICoverRepository
    {
        void Add(Cover entity);

        void Update(Cover entity);

        Cover Find(int id);

        void Remove(int id);

        void Remove(Cover entity);
    }
}