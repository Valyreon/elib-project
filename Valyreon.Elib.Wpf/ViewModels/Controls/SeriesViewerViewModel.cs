using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Valyreon.Elib.DataLayer.Filters;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Mvvm.Messaging;
using Valyreon.Elib.Wpf.BindingItems;
using Valyreon.Elib.Wpf.Messages;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class SeriesViewerViewModel : ViewModelBase, IViewer
    {
        private readonly IUnitOfWorkFactory uowFactory;
        private Action backAction;
        private string caption = "Series";
        private int collectionComboBoxSelectedIndex;
        private bool isAscendingSortDirection;
        private volatile bool isLoading;
        private bool isResultEmpty;
        private string searchText;
        private ObservableCollection<BookSeries> series;
        private int sortComboBoxSelectedIndex;

        public SeriesViewerViewModel(Filter filter, IUnitOfWorkFactory uowFactory)
        {
            Filter = filter;
            this.uowFactory = uowFactory;
            MessengerInstance.Register<RefreshCurrentViewMessage>(this, _ =>
            {
                if (!isLoading)
                {
                    Refresh();
                }
            });

            sortComboBoxSelectedIndex = Filter.SortByName ? 0 : 1;
            isAscendingSortDirection = Filter.Ascending;
            searchText = Filter.SearchText;
        }

        public Action Back
        {
            get => backAction;
            set
            {
                Set(() => Back, ref backAction, value);
                RaisePropertyChanged(() => IsBackEnabled);
            }
        }

        public ICommand BackCommand => new RelayCommand(Back);

        public string Caption
        {
            get => caption;
            set => Set(() => Caption, ref caption, value);
        }

        public IEnumerable<FilterComboBoxOption<Filter>> CollectionComboBoxOptions { set; get; }

        public int CollectionComboBoxSelectedIndex
        {
            get => collectionComboBoxSelectedIndex;
            set
            {
                Set(() => CollectionComboBoxSelectedIndex, ref collectionComboBoxSelectedIndex, value);
                Filter = CollectionComboBoxOptions.ElementAt(value).TransformFilter(Filter);
            }
        }

        public Filter Filter { get; set; }
        public ICommand GoToSeries => new RelayCommand<BookSeries>(a => Messenger.Default.Send(new SeriesSelectedMessage(a)));
        public bool IsAscendingSortDirection { get => isAscendingSortDirection; set => Set(() => IsAscendingSortDirection, ref isAscendingSortDirection, value); }
        public bool IsBackEnabled => Back != null;

        public bool IsResultEmpty
        {
            get => isResultEmpty;
            set => Set(() => IsResultEmpty, ref isResultEmpty, value);
        }

        public ICommand RefreshCommand => new RelayCommand(Refresh);

        public string SearchText
        {
            get => searchText;
            set
            {
                Set(() => SearchText, ref searchText, value);
                Filter = Filter with { SearchText = value };
                Refresh();
            }
        }

        public ObservableCollection<BookSeries> Series { get => series; set => Set(() => Series, ref series, value); }
        public IEnumerable<FilterComboBoxOption<Filter>> SortComboBoxOptions { get; } = FilterComboBoxOption<Filter>.AuthorSortFilterOptions;

        public int SortComboBoxSelectedIndex
        {
            get => sortComboBoxSelectedIndex;
            set
            {
                Set(() => SortComboBoxSelectedIndex, ref sortComboBoxSelectedIndex, value);
                Filter = SortComboBoxOptions.ElementAt(value).TransformFilter(Filter);
            }
        }

        public ICommand SortDirectionChangedCommand => new RelayCommand<bool>(HandleSortDirectionChange);

        public void Clear()
        {
            Series.Clear();
        }

        public void Dispose()
        {
            MessengerInstance.Unregister(this);
        }

        public Func<IViewer> GetCloneFunction()
        {
            var f = Filter with { };
            return () => new SeriesViewerViewModel(f, uowFactory);
        }

        public IFilterParameters GetFilter()
        {
            return Filter;
        }

        public void Refresh()
        {
            isLoading = true;
            LoadSeries();
        }

        private void HandleSortDirectionChange(bool isAscending)
        {
            Filter = Filter with { Ascending = isAscending };
            Refresh();
        }

        private async void LoadSeries()
        {
            using var uow = await uowFactory.CreateAsync();

            await SetupCollectionOptions(uow);

            var result = await uow.SeriesRepository.GetSeriesWithNumberOfBooks(Filter);

            if (!result.Any())
            {
                Series = new ObservableCollection<BookSeries>();
                IsResultEmpty = true;
                return;
            }

            Series = new ObservableCollection<BookSeries>(result);
            isLoading = false;
        }

        private async Task SetupCollectionOptions(IUnitOfWork uow)
        {
            var collections = await uow.CollectionRepository.GetAllAsync();
            var collectionOptions = collections.OrderBy(c => c.Tag).Select(c => new FilterComboBoxOption<Filter>
            {
                Name = c.Tag,
                TransformFilter = f => f with { CollectionId = c.Id },
            }).ToList();

            collectionOptions = collectionOptions.Prepend(new FilterComboBoxOption<Filter> { Name = "ALL", TransformFilter = f => f with { CollectionId = null } }).ToList();

            CollectionComboBoxOptions = collectionOptions;

            if (Filter.CollectionId.HasValue && Filter.CollectionId.Value > 0 && collectionComboBoxSelectedIndex == 0)
            {
                var selectedCollection = collections.Single(c => c.Id == Filter.CollectionId.Value);
                var option = collectionOptions.Single(o => o.Name == selectedCollection.Tag);
                collectionComboBoxSelectedIndex = collectionOptions.IndexOf(option);
            }
        }
    }
}
