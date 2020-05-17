using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace OnlineBookApi
{
    public interface IOnline
    {
        byte[] GetCover(Book book);

        Task<byte[]> GetCoverAsync(Book book);

        Task<IList<byte[]>> GetMultipleCoversAsync(Book book);
    }
}