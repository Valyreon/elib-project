using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DataLayer;
using Domain;
using ElibWpf.BindingItems;
using ElibWpf.CustomDataStructures;
using ElibWpf.Messages;
using ElibWpf.Models;
using MahApps.Metro.IconPacks;
using MVVMLibrary;

namespace ElibWpf.ViewModels.Controls
{
    public class BooksTabViewModel : ViewModelBase, ITabViewModel
    {
        private readonly PaneMainItem selectedMainItem;
        private readonly Selector selector;
        private readonly ViewerHistory history = new ViewerHistory();
        private string caption = "Books";
        private IViewer currentViewer;
        private bool isInSearchResults;
        private bool isSelectedMainAdded;

        private SearchParameters searchOptions;
        private UserCollection selectedCollection;
        private PaneMainItem selectedMainPaneItem;

        public BooksTabViewModel()
        {
            selector = new Selector();
            selectedMainItem = new PaneMainItem("Selected", PackIconFontAwesomeKind.CheckDoubleSolid, "Selected Books", new FilterParameters { Selected = true });

            MessengerInstance.Register<AuthorSelectedMessage>(this, HandleAuthorSelection);
            MessengerInstance.Register<BookSelectedMessage>(this, HandleBookChecked);
            MessengerInstance.Register<SeriesSelectedMessage>(this, HandleSeriesSelection);
            MessengerInstance.Register<CollectionSelectedMessage>(this, HandleCollectionSelection);
            MessengerInstance.Register<GoBackMessage>(this, x => GoToPreviousViewer());
            MessengerInstance.Register<ResetPaneSelectionMessage>(this, x =>
            {
                SelectedMainPaneItem = MainPaneItems[0];
                PaneSelectionChanged();
            });
            MessengerInstance.Register<RefreshSidePaneCollectionsMessage>(this,
                x => RaisePropertyChanged(() => Collections));

            MainPaneItems = new ObservableCollection<PaneMainItem>
            {
                new PaneMainItem("All", PackIconBoxIconsKind.SolidBook, "All Books", null),
                new PaneMainItem("Favorite", PackIconFontAwesomeKind.StarSolid, "Favorite Books", new FilterParameters { Favorite = true }),
                new PaneMainItem("Authors", PackIconFontAwesomeKind.PersonBoothSolid, "Authors", null)
            };
            SelectedMainPaneItem = MainPaneItems[0];
            PaneSelectionChanged();
            SearchOptions = new SearchParameters();
        }

        public ObservableCollection<UserCollection> Collections
        {
            get
            {
                using var uow = App.UnitOfWorkFactory.Create();
                return new ObservableCollection<UserCollection>(uow.CollectionRepository.All());
            }
        }

        public IViewer CurrentViewer
        {
            get => currentViewer;
            set => Set(() => CurrentViewer, ref currentViewer, value);
        }

        public bool IsBackEnabled => history.Count > 0;

        public ObservableCollection<PaneMainItem> MainPaneItems { get; set; }

        public ICommand PaneSelectionChangedCommand => new RelayCommand(PaneSelectionChanged);

        public ICommand SearchCheckboxChangedCommand => new RelayCommand(ProcessSearchCheckboxChanged);

        public ICommand SearchCommand => new RelayCommand<string>(ProcessSearchInput);

        public SearchParameters SearchOptions
        {
            get => searchOptions;
            set => Set(() => SearchOptions, ref searchOptions, value);
        }

        public UserCollection SelectedCollection
        {
            get => selectedCollection;

            set
            {
                Set(() => SelectedCollection, ref selectedCollection, value);
                if (selectedCollection != null)
                {
                    history.Clear();
                    UnitOfWork.ClearCache();

                    var filter = new FilterParameters
                    {
                        CollectionId = selectedCollection.Id
                    };

                    CurrentViewer = new BookViewerViewModel(filter, selector)
                    {
                        Caption = $"Collection {selectedCollection.Tag}"
                    };
                }
            }
        }

        public PaneMainItem SelectedMainPaneItem
        {
            get => selectedMainPaneItem;
            set => Set(() => SelectedMainPaneItem, ref selectedMainPaneItem, value);
        }

        public string Caption
        {
            get => caption;
            set => Set(() => Caption, ref caption, value);
        }

        private void HandleBookChecked(BookSelectedMessage obj)
        {
            if (selector.Count > 0 && !isSelectedMainAdded)
            {
                MainPaneItems.Add(selectedMainItem);
                isSelectedMainAdded = true;
            }
            else if (selector.Count == 0)
            {
                _ = MainPaneItems.Remove(selectedMainItem);
                isSelectedMainAdded = false;
            }
        }

        private void HandleCollectionSelection(CollectionSelectedMessage message)
        {
            SelectedCollection = Collections.FirstOrDefault(c => c.Id == message.CollectionId);
        }

        private async void ProcessSearchInput(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                token = token.ToLower();
                SearchOptions.Token = token;

                var resultViewModel = await CurrentViewer.Search(SearchOptions);

                if (resultViewModel == null)
                {
                    MessengerInstance.Send(new ShowDialogMessage("No matches", "No books found matching the search conditions."));
                }
                else
                {
                    resultViewModel.Back = GoToPreviousViewer;
                    var temp = currentViewer;
                    CurrentViewer = resultViewModel;
                    if (!isInSearchResults)
                    {
                        history.Push(temp);
                    }

                    isInSearchResults = true;
                }
            }
        }

        private void ProcessSearchCheckboxChanged()
        {
            if (!SearchOptions.SearchByTitle && !SearchOptions.SearchByAuthor && !SearchOptions.SearchBySeries)
            {
                SearchOptions = new SearchParameters();
            }
        }

        private void GoToPreviousViewer()
        {
            if (!IsBackEnabled)
            {
                return;
            }

            isInSearchResults = false;
            SearchOptions.Token = "";
            CurrentViewer = history.Pop();
        }

        private void HandleAuthorSelection(AuthorSelectedMessage obj)
        {
            var viewerCaption = $"Books by {obj.Author.Name}";
            if (viewerCaption == CurrentViewer.Caption)
            {
                return;
            }

            var temp = CurrentViewer;

            var filter = new FilterParameters
            {
                AuthorId = obj.Author.Id
            };

            CurrentViewer = new BookViewerViewModel(filter, selector)
            {
                Caption = viewerCaption,
                Back = GoToPreviousViewer
            };

            history.Push(temp);
        }

        private async void HandleSeriesSelection(SeriesSelectedMessage obj)
        {
            if (obj.Series == null)
            {
                return;
            }

            var viewerCaption = $"{obj.Series.Name} Series";
            if (viewerCaption == CurrentViewer.Caption)
            {
                return;
            }

            var filter = new FilterParameters
            {
                SeriesId = obj.Series.Id
            };

            var temp = CurrentViewer;

            CurrentViewer = new BookViewerViewModel(filter, selector)
            {
                Caption = viewerCaption,
                Back = GoToPreviousViewer
            };

            history.Push(temp);
        }

        private async void PaneSelectionChanged()
        {
            if (SelectedMainPaneItem == null)
            {
                return;
            }

            history.Clear();

            var filter = SelectedMainPaneItem.Filter?.Clone() ?? new FilterParameters();

            UnitOfWork.ClearCache();

            if (SelectedMainPaneItem.PaneCaption == "Authors")
            {
                CurrentViewer = new AuthorViewerViewModel()
                {
                    Caption = SelectedMainPaneItem.PaneCaption
                };
            }
            else
            {
                CurrentViewer = new BookViewerViewModel(filter, selector)
                {
                    Caption = SelectedMainPaneItem.ViewerCaption
                };
            }
        }
    }
}
