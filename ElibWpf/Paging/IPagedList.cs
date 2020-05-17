namespace ElibWpf.Paging
{
    public interface IPagedList
    {
        int Page { get; }
        int PageCount { get; }
        int PageSize { get; }
        int TotalCount { get; }
    }
}