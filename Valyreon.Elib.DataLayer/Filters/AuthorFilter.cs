namespace Valyreon.Elib.DataLayer.Filters
{
    public record Filter : IFilterParameters
    {
        public string SearchText { get; set; }
        public int? CollectionId { get; set; }
        public bool SortByName { get; set; } = true;
        public bool SortByBookCount { get; set; }
        public bool Ascending { get; set; } = true;
    }
}
