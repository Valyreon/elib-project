using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Domain;
using OnlineBookApi.OpenLibrary.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

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
            SearchQuery searchQuery = await Query.GeneralQueryAsync(book.Title + " " + string.Join(" ", book.Authors?.Select(x => x.Name)), Query.SearchType.Query);

            IList<byte[]> result = new List<byte[]>();

            foreach (var x in searchQuery.docs)
                result.Add(Query.GetImageFromImageIdAsync(x.cover_i.ToString()).Result);

            return result;
        }
    }
}
