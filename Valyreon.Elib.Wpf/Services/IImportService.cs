using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.Wpf.Services
{
    public interface IImportService
    {
        Task<IReadOnlyList<string>> GetNotImportedBookPathsAsync();

        Task ImportBookAsync(Book book);
    }
}
