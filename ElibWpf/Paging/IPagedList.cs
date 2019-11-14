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