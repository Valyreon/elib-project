using System;
using System.Collections.Generic;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface ICache<T> where T : ObservableEntity
    {
        void ClearCache();

        public IEnumerable<T> GetCachedObjects();

        bool IsCached(Func<T, bool> entity);
    }
}
