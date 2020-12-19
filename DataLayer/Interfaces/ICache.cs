using System.Collections.Generic;
using Domain;

namespace DataLayer.Interfaces
{
    public interface ICache<T> where T : ObservableEntity
    {
        void ClearCache();
        public IEnumerable<T> GetCachedObjects();
    }
}
