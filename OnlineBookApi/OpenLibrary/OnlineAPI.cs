using Domain;
using OnlineBookApi.OpenLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookApi.OpenLibrary
{
    public class OnlineAPI : IOnline
    {
        public byte[] GetCover(Book book)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetCoverAsync(Book book)
        {
            // TODO: Isbn check

            throw new NotImplementedException();
        }

        public async Task<IList<byte[]>> GetMultipleCoversAsync(Book book)
        {
            SearchQuery searchQuery = await Query.GeneralQueryAsync(
                book.Title + " " + string.Join(" ", book.Authors?.Select(x => x.Name)), Query.SearchType.Query);

            return searchQuery.docs.Select(x => Query.GetImageFromImageIdAsync(x.cover_i.ToString()).Result).ToList();
        }
    }
}