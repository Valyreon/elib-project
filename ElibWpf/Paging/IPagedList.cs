using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.Paging
{
    public interface IPagedList
    {
        int TotalCount { get; }
        int PageCount { get; }
        int Page { get; }
        int PageSize { get; }
    }

}
