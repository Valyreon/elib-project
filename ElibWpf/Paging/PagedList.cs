using System.Collections.Generic;
using System.Linq;

namespace ElibWpf.Paging
{
    public class PagedList<T> : List<T>, IPagedList
    {
        public PagedList(IQueryable<T> source, int page, int pageSize)
        {
            this.TotalCount = source.Count();
            this.PageCount = GetPageCount(pageSize, this.TotalCount);
            this.Page = page < 1 ? 0 : page - 1;
            this.PageSize = pageSize;

            this.AddRange(source.Skip(this.Page * this.PageSize).Take(this.PageSize).ToList());
        }

        public int Page { get; }
        public int PageCount { get; }
        public int PageSize { get; }
        public int TotalCount { get; }

        private static int GetPageCount(int pageSize, int totalCount)
        {
            if (pageSize == 0)
            {
                return 0;
            }

            int remainder = totalCount % pageSize;
            return totalCount / pageSize + (remainder == 0 ? 0 : 1);
        }
    }
}