using System.Collections.Generic;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Extensions
{
    internal static class DictionaryExtensions
    {
        internal static IEnumerable<V> FilterAndUpdateCache<V>(this IDictionary<int, V> dictionary, IEnumerable<V> dbResult) where V : ObservableEntity
        {
            foreach (var uc in dbResult)
            {
                if (dictionary.TryGetValue(uc.Id, out var bookFromCache))
                {
                    yield return bookFromCache;
                }
                else
                {
                    dictionary.Add(uc.Id, uc);
                    yield return uc;
                }
            }
        }

        internal static V FilterAndUpdateCache<V>(this IDictionary<int, V> dictionary, V dbResult) where V : ObservableEntity
        {
            if (dbResult == null)
            {
                return null;
            }

            if (dictionary.TryGetValue(dbResult.Id, out var bookFromCache))
            {
                return bookFromCache;
            }
            else
            {
                dictionary.Add(dbResult.Id, dbResult);
                return dbResult;
            }
        }
    }
}
