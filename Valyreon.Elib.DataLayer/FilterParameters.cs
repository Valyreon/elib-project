namespace Valyreon.Elib.DataLayer
{
    public record FilterParameters
    {
        public int? AuthorId { get; set; }
        public int? SeriesId { get; set; }
        public int? CollectionId { get; set; }

        public bool? Read { get; set; }
        public bool? Favorite { get; set; }
        public bool? Selected { get; set; }

        public bool SortByTitle { get; set; }
        public bool SortByImportOrder { get; set; } = true;
        public bool SortBySeries { get; set; }
        public bool SortByAuthor { get; set; }
        public bool Ascending { get; set; }

        public string SearchText { get; set; }
    }
}
