namespace DataLayer
{
    public class FilterParameters
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
        public bool Ascending { get; set; } = false;

        public SearchParameters SearchParameters { get; set; }

        public FilterParameters Clone()
        {
            return new FilterParameters
            {
                SearchParameters = this.SearchParameters?.Clone(),
                AuthorId = this.AuthorId,
                SeriesId = this.SeriesId,
                CollectionId = this.CollectionId,
                Read = this.Read,
                Favorite = this.Favorite,
                Selected = this.Selected,
                SortByTitle = this.SortByTitle,
                SortByImportOrder = this.SortByImportOrder,
                SortByAuthor = this.SortByAuthor,
                SortBySeries = this.SortBySeries,
                Ascending = this.Ascending
            };
        }
    }
}
