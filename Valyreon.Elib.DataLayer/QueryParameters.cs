namespace Valyreon.Elib.DataLayer
{
    /// <summary>
    /// Defines sort property and direction, used for building queries.
    /// NOTE: Container must be indexed by that property otherwise ORDER BY will fail.
    /// </summary>
    public record Sort
    {
        public string PropertyName { get; set; }
        public bool IsAscending { get; set; } = true;

        public Sort()
        { }

        public Sort(string propertyName, bool isAscending)
        {
            PropertyName = propertyName;
            IsAscending = isAscending;
        }
    }

    /// <summary>
    /// Defines paging and sorting, used for building queries.
    /// </summary>
    public record QueryParameters
    {
        public int PageSize { get; set; }

        /// <summary>
        /// Page number. Starts with 0. Adjust for that.
        /// </summary>
        public int Page { get; set; }

        public Sort SortBy { get; set; }

        public bool HasPaging()
        {
            return PageSize > 0 && Page > -1;
        }

        public int GetOffset()
        {
            return Page * PageSize;
        }

        public bool HasSorting()
        {
            return SortBy != null && !string.IsNullOrWhiteSpace(SortBy.PropertyName);
        }
    }
}
