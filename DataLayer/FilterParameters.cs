using System.Collections.Generic;

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
                SearchParameters = SearchParameters?.Clone(),
                AuthorId = AuthorId,
                SeriesId = SeriesId,
                CollectionId = CollectionId,
                Read = Read,
                Favorite = Favorite,
                Selected = Selected,
                SortByTitle = SortByTitle,
                SortByImportOrder = SortByImportOrder,
                SortByAuthor = SortByAuthor,
                SortBySeries = SortBySeries,
                Ascending = Ascending
            };
        }

        public override bool Equals(object obj)
        {
            return obj is FilterParameters parameters &&
                   AuthorId == parameters.AuthorId &&
                   SeriesId == parameters.SeriesId &&
                   CollectionId == parameters.CollectionId &&
                   Read == parameters.Read &&
                   Favorite == parameters.Favorite &&
                   Selected == parameters.Selected &&
                   SortByTitle == parameters.SortByTitle &&
                   SortByImportOrder == parameters.SortByImportOrder &&
                   SortBySeries == parameters.SortBySeries &&
                   SortByAuthor == parameters.SortByAuthor &&
                   Ascending == parameters.Ascending &&
                   EqualityComparer<SearchParameters>.Default.Equals(SearchParameters, parameters.SearchParameters);
        }

        public override int GetHashCode()
        {
            var hashCode = -1485931979;
            hashCode = (hashCode * -1521134295) + AuthorId.GetHashCode();
            hashCode = (hashCode * -1521134295) + SeriesId.GetHashCode();
            hashCode = (hashCode * -1521134295) + CollectionId.GetHashCode();
            hashCode = (hashCode * -1521134295) + Read.GetHashCode();
            hashCode = (hashCode * -1521134295) + Favorite.GetHashCode();
            hashCode = (hashCode * -1521134295) + Selected.GetHashCode();
            hashCode = (hashCode * -1521134295) + SortByTitle.GetHashCode();
            hashCode = (hashCode * -1521134295) + SortByImportOrder.GetHashCode();
            hashCode = (hashCode * -1521134295) + SortBySeries.GetHashCode();
            hashCode = (hashCode * -1521134295) + SortByAuthor.GetHashCode();
            hashCode = (hashCode * -1521134295) + Ascending.GetHashCode();
            hashCode = (hashCode * -1521134295) + EqualityComparer<SearchParameters>.Default.GetHashCode(SearchParameters);
            return hashCode;
        }
    }
}
