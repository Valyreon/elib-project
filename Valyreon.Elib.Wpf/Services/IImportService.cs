using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.Wpf.Models;

namespace Valyreon.Elib.Wpf.Services
{
    public interface IImportService
    {
        Task<IEnumerable<string>> ImportAsync();
        Task<IEnumerable<string>> ImportAsync(SourcePath sourcePath);
    }
}
