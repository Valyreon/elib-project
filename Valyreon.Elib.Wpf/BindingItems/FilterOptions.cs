using System;
using Valyreon.Elib.Mvvm;

namespace Valyreon.Elib.Wpf.BindingItems
{
    public class FilterOptions : ObservableObject, ICloneable
    {
        private bool ascending = false;

        private string fileTypeFilter = "epub|mobi";

        private bool showAll;

        private bool showAllTypes = true;

        private bool showRead;

        private bool showUnread;

        private bool sortByAuthor;

        private bool sortByImportTime;

        private bool sortBySeries;

        private bool sortByTitle;

        public FilterOptions()
        {
            ShowAll = true;
            SortByImportTime = true;
        }

        public bool Ascending
        {
            get => ascending;
            set => Set(() => Ascending, ref ascending, value);
        }

        public string FileTypeFilter
        {
            get => fileTypeFilter;
            set => Set(() => FileTypeFilter, ref fileTypeFilter, value);
        }

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

        public bool ShowAllTypes
        {
            get => showAllTypes;
            set => Set(() => ShowAllTypes, ref showAllTypes, value);
        }

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

        private void RaiseReadProperties()
        {
            RaisePropertyChanged(() => ShowAll);
            RaisePropertyChanged(() => ShowRead);
            RaisePropertyChanged(() => ShowUnread);
        }

        private void RaiseSortPropertiesChange()
        {
            RaisePropertyChanged(() => SortByTitle);
            RaisePropertyChanged(() => SortByAuthor);
            RaisePropertyChanged(() => SortBySeries);
            RaisePropertyChanged(() => SortByImportTime);
        }

        /* Sort options */
        /* Read filter */
        /* File type filter */
    }
}
