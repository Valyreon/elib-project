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

        public override bool Equals(object obj)
        {
            return obj is FilterParameters parameters &&
                   this.AuthorId == parameters.AuthorId &&
                   this.SeriesId == parameters.SeriesId &&
                   this.CollectionId == parameters.CollectionId &&
                   this.Read == parameters.Read &&
                   this.Favorite == parameters.Favorite &&
                   this.Selected == parameters.Selected &&
                   this.SortByTitle == parameters.SortByTitle &&
                   this.SortByImportOrder == parameters.SortByImportOrder &&
                   this.SortBySeries == parameters.SortBySeries &&
                   this.SortByAuthor == parameters.SortByAuthor &&
                   this.Ascending == parameters.Ascending &&
                   EqualityComparer<SearchParameters>.Default.Equals(this.SearchParameters, parameters.SearchParameters);
        }

        public override int GetHashCode()
        {
            int hashCode = -1485931979;
            hashCode = hashCode * -1521134295 + this.AuthorId.GetHashCode();
            hashCode = hashCode * -1521134295 + this.SeriesId.GetHashCode();
            hashCode = hashCode * -1521134295 + this.CollectionId.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Read.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Favorite.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Selected.GetHashCode();
            hashCode = hashCode * -1521134295 + this.SortByTitle.GetHashCode();
            hashCode = hashCode * -1521134295 + this.SortByImportOrder.GetHashCode();
            hashCode = hashCode * -1521134295 + this.SortBySeries.GetHashCode();
            hashCode = hashCode * -1521134295 + this.SortByAuthor.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Ascending.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<SearchParameters>.Default.GetHashCode(this.SearchParameters);
            return hashCode;
        }
    }
}