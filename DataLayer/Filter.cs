using System;

namespace DataLayer
{
    public class Filter
    {
        public string Token { get; set; }
        public bool SearchByAuthor { get; set; }
        public bool SearchByName { get; set; } = true;
        public bool SearchBySeries { get; set; }

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

        public Filter Clone => new Filter
        {
            Token = this.Token,
            SearchByAuthor = this.SearchByAuthor,
            SearchByName = this.SearchByName,
            SearchBySeries = this.SearchBySeries,
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
