using System.Collections.Generic;

namespace Valyreon.Elib.DataLayer
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
            var hash = new System.HashCode();
            hash.Add(AuthorId);
            hash.Add(SeriesId);
            hash.Add(CollectionId);
            hash.Add(Read);
            hash.Add(Favorite);
            hash.Add(Selected);
            hash.Add(SortByTitle);
            hash.Add(SortByImportOrder);
            hash.Add(SortBySeries);
            hash.Add(SortByAuthor);
            hash.Add(Ascending);
            hash.Add(SearchParameters);
            return hash.ToHashCode();
        }
    }
}
