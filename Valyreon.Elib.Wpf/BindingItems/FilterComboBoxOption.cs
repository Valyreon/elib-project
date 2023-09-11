using System;
using System.Collections.Generic;
using Valyreon.Elib.DataLayer.Filters;

namespace Valyreon.Elib.Wpf.BindingItems
{
    public class FilterComboBoxOption<T> where T : IFilterParameters
    {
        public static readonly IEnumerable<FilterComboBoxOption<Filter>> AuthorSortFilterOptions = new List<FilterComboBoxOption<Filter>>()
        {
            new FilterComboBoxOption<Filter>
            {
                Name = "NAME",
                TransformFilter = filter => filter with { SortByName = true, SortByBookCount = false }
            },
            new FilterComboBoxOption<Filter>
            {
                Name = "BOOK COUNT",
                TransformFilter = filter => filter with { SortByBookCount = true, SortByName = false }
            }
        };

        public static readonly IEnumerable<FilterComboBoxOption<BookFilter>> BookSortFilterOptions = new List<FilterComboBoxOption<BookFilter>>()
        {
            new FilterComboBoxOption<BookFilter>
            {
                Name = "IMPORT TIME",
                TransformFilter = filter => filter with { SortByImportOrder = true, SortByAuthor = false, SortBySeries = false, SortByTitle = false }
            },
            new FilterComboBoxOption<BookFilter>
            {
                Name = "TITLE",
                TransformFilter = filter => filter with { SortByImportOrder = false, SortByAuthor = false, SortBySeries = false, SortByTitle = true }
            },
            new FilterComboBoxOption<BookFilter>
            {
                Name = "AUTHOR",
                TransformFilter = filter => filter with { SortByImportOrder = false, SortByAuthor = true, SortBySeries = false, SortByTitle = false }
            },
            new FilterComboBoxOption<BookFilter>
            {
                Name = "SERIES",
                TransformFilter = filter => filter with { SortByImportOrder = false, SortByAuthor = false, SortBySeries = true, SortByTitle = false }
            },
        };

        public static readonly IEnumerable<FilterComboBoxOption<BookFilter>> BookStatusFilterOptions = new List<FilterComboBoxOption<BookFilter>>()
        {
            new FilterComboBoxOption<BookFilter>
            {
                Name = "ALL",
                TransformFilter = filter => filter with { Read = null }
            },
            new FilterComboBoxOption<BookFilter>
            {
                Name = "READ",
                TransformFilter = filter => filter with { Read = true }
            },
            new FilterComboBoxOption<BookFilter>
            {
                Name = "UNREAD",
                TransformFilter = filter => filter with { Read = false }
            },
        };

        public string Name { get; set; }
        public Func<T, T> TransformFilter { get; set; }
    }
}
