using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Domain;

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
    }
}
