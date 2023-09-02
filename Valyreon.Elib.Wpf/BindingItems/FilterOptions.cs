using System;
using Valyreon.Elib.Mvvm;

namespace Valyreon.Elib.Wpf.BindingItems
{
    public class FilterOptions : ObservableObject, ICloneable
    {
        private void RaiseSortPropertiesChange()
        {
            RaisePropertyChanged(() => SortByTitle);
            RaisePropertyChanged(() => SortByAuthor);
            RaisePropertyChanged(() => SortBySeries);
            RaisePropertyChanged(() => SortByImportTime);
        }

        /* Sort options */
        private bool sortByTitle;

        public bool SortByTitle
        {
            get => sortByTitle;

            set
            {
                sortByTitle = value;
                if (sortByTitle)
                {
                    SortByAuthor = false;
                    SortBySeries = false;
                    SortByImportTime = false;
                }
                RaiseSortPropertiesChange();
            }
        }

        private bool sortByAuthor;

        public bool SortByAuthor
        {
            get => sortByAuthor;

            set
            {
                sortByAuthor = value;
                if (sortByAuthor)
                {
                    SortByTitle = false;
                    SortBySeries = false;
                    SortByImportTime = false;
                }
                RaiseSortPropertiesChange();
            }
        }

        private bool sortBySeries;

        public bool SortBySeries
        {
            get => sortBySeries;

            set
            {
                sortBySeries = value;
                if (sortBySeries)
                {
                    SortByTitle = false;
                    SortByAuthor = false;
                    SortByImportTime = false;
                }
                RaiseSortPropertiesChange();
            }
        }

        private bool sortByImportTime;

        public bool SortByImportTime
        {
            get => sortByImportTime;

            set
            {
                sortByImportTime = value;
                if (sortByImportTime)
                {
                    SortByTitle = false;
                    SortByAuthor = false;
                    SortBySeries = false;
                }
                RaiseSortPropertiesChange();
            }
        }

        private bool ascending = false;

        public bool Ascending
        {
            get => ascending;
            set => Set(() => Ascending, ref ascending, value);
        }

        private void RaiseReadProperties()
        {
            RaisePropertyChanged(() => ShowAll);
            RaisePropertyChanged(() => ShowRead);
            RaisePropertyChanged(() => ShowUnread);
        }

        public object Clone()
        {
            return new FilterOptions
            {
                SortByAuthor = SortByAuthor,
                SortByImportTime = SortByImportTime,
                SortBySeries = SortBySeries,
                SortByTitle = SortByTitle,
                ShowAll = ShowAll,
                ShowRead = ShowRead,
                ShowUnread = ShowUnread,
                Ascending = Ascending,
                FileTypeFilter = FileTypeFilter,
                ShowAllTypes = ShowAllTypes
            };
        }

        /* Read filter */
        private bool showAll;

        public bool ShowAll
        {
            get => showAll;

            set
            {
                showAll = value;
                if (showAll)
                {
                    ShowRead = false;
                    ShowUnread = false;
                }
                RaiseReadProperties();
            }
        }

        private bool showRead;

        public bool ShowRead
        {
            get => showRead;

            set
            {
                showRead = value;
                if (showRead)
                {
                    ShowAll = false;
                    ShowUnread = false;
                }
                RaiseReadProperties();
            }
        }

        private bool showUnread;

        public bool ShowUnread
        {
            get => showUnread;

            set
            {
                showUnread = value;
                if (showUnread)
                {
                    ShowAll = false;
                    ShowRead = false;
                }
                RaiseReadProperties();
            }
        }

        private bool showAllTypes = true;

        public bool ShowAllTypes
        {
            get => showAllTypes;
            set => Set(() => ShowAllTypes, ref showAllTypes, value);
        }

        /* File type filter */
        private string fileTypeFilter = "epub|mobi";

        public string FileTypeFilter
        {
            get => fileTypeFilter;
            set => Set(() => FileTypeFilter, ref fileTypeFilter, value);
        }

        public FilterOptions()
        {
            ShowAll = true;
            SortByImportTime = true;
        }
    }
}
