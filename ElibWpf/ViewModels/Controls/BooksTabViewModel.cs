using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DataLayer;
using Domain;
using ElibWpf.BindingItems;
using ElibWpf.CustomDataStructures;
using ElibWpf.Messages;
using MahApps.Metro.IconPacks;
using Models;
using MVVMLibrary;

namespace ElibWpf.ViewModels.Controls
{
    public class BooksTabViewModel : ViewModelBase, ITabViewModel
    {
        private readonly PaneMainItem selectedMainItem;
        private readonly Selector selector;
        private readonly ViewerHistory History = new ViewerHistory();
        private string caption = "Books";
        private IViewer currentViewer;
        private bool isInSearchResults;
        private bool isSelectedMainAdded;

        private SearchParameters searchOptions;
        private UserCollection selectedCollection;
        private PaneMainItem selectedMainPaneItem;

        public BooksTabViewModel()
        {
            this.selector = new Selector();
            this.selectedMainItem = new PaneMainItem("Selected", PackIconFontAwesomeKind.CheckDoubleSolid, "Selected Books", new FilterParameters { Selected = true });

            this.MessengerInstance.Register<AuthorSelectedMessage>(this, this.HandleAuthorSelection);
            this.MessengerInstance.Register<BookSelectedMessage>(this, this.HandleBookChecked);
            this.MessengerInstance.Register<SeriesSelectedMessage>(this, this.HandleSeriesSelection);
            this.MessengerInstance.Register<CollectionSelectedMessage>(this, this.HandleCollectionSelection);
            this.MessengerInstance.Register<GoBackMessage>(this, x => this.GoToPreviousViewer());
            this.MessengerInstance.Register<ResetPaneSelectionMessage>(this, x =>
            {
                this.SelectedMainPaneItem = this.MainPaneItems[0];
                this.PaneSelectionChanged();
            });
            this.MessengerInstance.Register<RefreshSidePaneCollectionsMessage>(this,
                x => { this.RaisePropertyChanged(() => this.Collections); });

            this.MainPaneItems = new ObservableCollection<PaneMainItem>
            {
                new PaneMainItem("All", PackIconBoxIconsKind.SolidBook, "All Books", null),
                new PaneMainItem("Favorite", PackIconFontAwesomeKind.StarSolid, "Favorite Books", new FilterParameters { Favorite = true })
            };
            this.SelectedMainPaneItem = this.MainPaneItems[0];
            this.PaneSelectionChanged();
            this.SearchOptions = new SearchParameters();
        }



        public ObservableCollection<UserCollection> Collections
        {
            get
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                return new ObservableCollection<UserCollection>(uow.CollectionRepository.All());
            }
        }

        public IViewer CurrentViewer
        {
            get => this.currentViewer;
            set => this.Set(() => CurrentViewer, ref this.currentViewer, value);
        }

        public bool IsBackEnabled => this.History.Count > 0;

        public ObservableCollection<PaneMainItem> MainPaneItems { get; set; }

        public ICommand PaneSelectionChangedCommand => new RelayCommand(this.PaneSelectionChanged);

        public ICommand SearchCheckboxChangedCommand => new RelayCommand(this.ProcessSearchCheckboxChanged);

        public ICommand SearchCommand => new RelayCommand<string>(this.ProcessSearchInput);

        public SearchParameters SearchOptions
        {
            get => this.searchOptions;
            set => this.Set(() => this.SearchOptions, ref this.searchOptions, value);
        }

        public UserCollection SelectedCollection
        {
            get => this.selectedCollection;

            set
            {
                this.Set(() => SelectedCollection, ref this.selectedCollection, value);
                if (this.selectedCollection != null)
                {
                    this.History.Clear();

                    using var uow = ApplicationSettings.CreateUnitOfWork();
                    uow.ClearCache();
                    uow.Dispose();

                    FilterParameters filter = new FilterParameters
                    {
                        CollectionId = selectedCollection.Id
                    };

                    this.CurrentViewer = new BookViewerViewModel(filter, this.selector)
                    {
                        Caption = $"Collection {this.selectedCollection.Tag}"
                    };
                }
            }
        }

        public PaneMainItem SelectedMainPaneItem
        {
            get => this.selectedMainPaneItem;
            set => this.Set(() => SelectedMainPaneItem, ref this.selectedMainPaneItem, value);
        }

        public string Caption
        {
            get => this.caption;
            set => this.Set(() => Caption, ref this.caption, value);
        }

        private void HandleBookChecked(BookSelectedMessage obj)
        {
            if (this.selector.Count > 0 && !this.isSelectedMainAdded)
            {
                this.MainPaneItems.Add(this.selectedMainItem);
                this.isSelectedMainAdded = true;
            }
            else if (this.selector.Count == 0)
            {
                this.MainPaneItems.Remove(this.selectedMainItem);
                this.isSelectedMainAdded = false;
            }
        }

        private void HandleCollectionSelection(CollectionSelectedMessage message)
        {
            this.SelectedCollection = this.Collections.FirstOrDefault(c => c.Id == message.CollectionId);
        }

        private void ProcessSearchInput(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                token = token.ToLower();
                this.SearchOptions.Token = token;

                var resultViewModel = this.CurrentViewer.Search(this.SearchOptions);

                if(resultViewModel == null)
                {
                    this.MessengerInstance.Send(new ShowDialogMessage("No matches", "No books found matching the search conditions."));
                }
                else
                {
                    resultViewModel.Back = this.GoToPreviousViewer;
                    var temp = this.currentViewer;
                    this.CurrentViewer = resultViewModel;
                    if (!isInSearchResults)
                        this.History.Push(temp);
                    this.isInSearchResults = true;
                }
            }
        }

        private void ProcessSearchCheckboxChanged()
        {
            if (!this.SearchOptions.SearchByTitle && !this.SearchOptions.SearchByAuthor && !this.SearchOptions.SearchBySeries)
            {
                this.SearchOptions = new SearchParameters();
            }
        }

        private void GoToPreviousViewer()
        {
            if (!this.IsBackEnabled)
            {
                return;
            }

            this.isInSearchResults = false;
            this.SearchOptions.Token = "";
            this.CurrentViewer = this.History.Pop();
        }

        private void HandleAuthorSelection(AuthorSelectedMessage obj)
        {
            using var uow = ApplicationSettings.CreateUnitOfWork();

            string viewerCaption = $"Books by {obj.Author.Name}";
            if (viewerCaption == this.CurrentViewer.Caption)
            {
                return;
            }

            var temp = this.CurrentViewer;

            FilterParameters filter = new FilterParameters
            {
                AuthorId = obj.Author.Id
            };

            this.CurrentViewer = new BookViewerViewModel(filter, this.selector)
            {
                Caption = viewerCaption,
                Back = this.GoToPreviousViewer
            };

            this.History.Push(temp);
        }

        private void HandleSeriesSelection(SeriesSelectedMessage obj)
        {
            if (obj.Series == null)
                return;

            using var uow = ApplicationSettings.CreateUnitOfWork();

            string viewerCaption = $"{obj.Series.Name} Series";
            if (viewerCaption == this.CurrentViewer.Caption)
            {
                return;
            }

            FilterParameters filter = new FilterParameters
            {
                SeriesId = obj.Series.Id
            };

            var temp = this.CurrentViewer;

            this.CurrentViewer = new BookViewerViewModel(filter, this.selector)
            {
                Caption = viewerCaption,
                Back = this.GoToPreviousViewer
            };

            this.History.Push(temp);
        }

        private void PaneSelectionChanged()
        {
            if (this.SelectedMainPaneItem == null)
            {
                return;
            }

            this.History.Clear();

            FilterParameters filter = SelectedMainPaneItem.Filter?.Clone() ?? new FilterParameters();

            using var uow = ApplicationSettings.CreateUnitOfWork();
            uow.ClearCache();
            uow.Dispose();

            this.CurrentViewer = new BookViewerViewModel(filter, this.selector)
            {
                Caption = this.SelectedMainPaneItem.ViewerCaption
            };
        }

    }
}