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
    public class AuthorViewerViewModel : ViewModelBase, IViewer, IDisposable
    {
        private bool isResultEmpty;
        private string caption = "Authors";

        public ObservableCollection<Author> Authors { get => authors; set => Set(() => Authors, ref authors, value); }

        public ICommand BackCommand => new RelayCommand(Back);

        public string Caption
        {
            get => caption;
            set => Set(() => Caption, ref caption, value);
        }

        private string searchText;

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

        public Filter Filter { get; set; }

        public IFilterParameters GetFilter()
        {
            return Filter;
        }

        public IEnumerable<FilterComboBoxOption<Filter>> CollectionComboBoxOptions { set; get; }
        public IEnumerable<FilterComboBoxOption<Filter>> SortComboBoxOptions { get; } = FilterComboBoxOption<Filter>.AuthorSortFilterOptions;

        public int CollectionComboBoxSelectedIndex
        {
            get => collectionComboBoxSelectedIndex;
            set
            {
                Set(() => CollectionComboBoxSelectedIndex, ref collectionComboBoxSelectedIndex, value);
                Filter = CollectionComboBoxOptions.ElementAt(value).TransformFilter(Filter);
            }
        }

        public int SortComboBoxSelectedIndex
        {
            get => sortComboBoxSelectedIndex;
            set
            {
                Set(() => SortComboBoxSelectedIndex, ref sortComboBoxSelectedIndex, value);
                Filter = SortComboBoxOptions.ElementAt(value).TransformFilter(Filter);
            }
        }

        public bool IsAscendingSortDirection { get => isAscendingSortDirection; set => Set(() => IsAscendingSortDirection, ref isAscendingSortDirection, value); }

        public ICommand SortDirectionChangedCommand => new RelayCommand<bool>(HandleSortDirectionChange);

        private void HandleSortDirectionChange(bool isAscending)
        {
            Filter = Filter with { Ascending = isAscending };
            Refresh();
        }

        private Action backAction;

        public Action Back
        {
            get => backAction;
            set
            {
                Set(() => Back, ref backAction, value);
                RaisePropertyChanged(() => IsBackEnabled);
            }
        }

        public bool IsBackEnabled => Back != null;

        public bool IsResultEmpty
        {
            get => isResultEmpty;
            set => Set(() => IsResultEmpty, ref isResultEmpty, value);
        }

        public AuthorViewerViewModel(Filter filter, IUnitOfWorkFactory uowFactory)
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

            sortComboBoxSelectedIndex = filter.SortByName ? 0 : 1;
            isAscendingSortDirection = filter.Ascending;
            searchText = filter.SearchText;
        }

        public ICommand RefreshCommand => new RelayCommand(Refresh);

        public async void Refresh()
        {
            isLoading = true;

            await LoadAuthors();
        }

        private volatile bool isLoading;
        private ObservableCollection<Author> authors;
        private int sortComboBoxSelectedIndex;
        private int collectionComboBoxSelectedIndex;
        private bool isAscendingSortDirection;
        private readonly IUnitOfWorkFactory uowFactory;

        public ICommand GoToAuthor => new RelayCommand<Author>(a => Messenger.Default.Send(new AuthorSelectedMessage(a)));

        private async Task LoadAuthors()
        {
            using var uow = await uowFactory.CreateAsync();

            await SetupCollectionOptions(uow);

            var result = await uow.AuthorRepository.GetAuthorsWithNumberOfBooks(Filter);

            if (!result.Any())
            {
                Authors = new ObservableCollection<Author>();
                IsResultEmpty = true;
                return;
            }

            Authors = new ObservableCollection<Author>(result);
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

            if(Filter.CollectionId.HasValue && Filter.CollectionId.Value > 0 && collectionComboBoxSelectedIndex == 0)
            {
                var selectedCollection = collections.Single(c => c.Id == Filter.CollectionId.Value);
                var option = collectionOptions.Single(o => o.Name == selectedCollection.Tag);
                collectionComboBoxSelectedIndex = collectionOptions.IndexOf(option);

            }
        }

        public void Clear()
        {
            Authors.Clear();
        }

        public Func<IViewer> GetCloneFunction()
        {
            var f = Filter with { };
            var fact = uowFactory;
            return () => new AuthorViewerViewModel(f, fact);
        }

        public void Dispose()
        {
            MessengerInstance.Unregister(this);
        }
    }
}
