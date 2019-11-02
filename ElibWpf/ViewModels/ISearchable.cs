using Models.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.ViewModels
{
    public interface ISearchable
    {
        void Search(string token, SearchOptions options = null);
    }
}
