using System.Collections.Generic;
using Domain;

namespace DataLayer.Extensions
{
    internal static class DictionaryExtensions
    {
        internal static IEnumerable<V> FilterAndUpdateCache<V>(this IDictionary<int, V> dictionary, IEnumerable<V> dbResult) where V : ObservableEntity
        {
            var result = new List<V>();

            foreach (var uc in dbResult)
            {
                if (dictionary.TryGetValue(uc.Id, out var bookFromCache))
                {
                    result.Add(bookFromCache);
                }
                else
                {
                    dictionary.Add(uc.Id, uc);
                    result.Add(uc);
                }
            }

            return result;
        }
    }
}
