using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineBookApi
{
    public interface IOnline
    {
        byte[] GetCover(Book book);

        Task<byte[]> GetCoverAsync(Book book);

        Task<IList<byte[]>> GetMultipleCoversAsync(Book book);
    }
}