using System.Collections.Generic;

namespace Models.Options
{
    public class Filter
    {
        public List<int> BookIds { get; set; }
        public List<int> AuthorIds { get; set; }
        public List<int> SeriesIds { get; set; }
        public List<int> CollectionIds { get; set; }

        public bool? Read { get; set; }
        public bool? Favorite { get; set; }
        public bool? Selected { get; set; }

        public bool SortByTitle { get; set; }
        public bool SortByImportOrder { get; set; } = true;
        public bool SortBySeries { get; set; }
        public bool SortByAuthor { get; set; }
        public bool Ascending { get; set; } = true;
    }
}
