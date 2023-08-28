using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Valyreon.Elib.DataLayer;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.BindingItems;
using Valyreon.Elib.Wpf.CustomDataStructures;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class BooksTabViewModel : ViewModelBase, ITabViewModel
    {
        private readonly PaneMainItem selectedMainItem;
        private readonly ViewerHistory history = new();
        private string caption = "Books";
        private IViewer currentViewer;

        private UserCollection selectedCollection;
        private PaneMainItem selectedMainPaneItem;

        public BooksTabViewModel()
        {
            selectedMainItem = new PaneMainItem("Selected", "Selected Books", new FilterParameters { Selected = true });

            MessengerInstance.Register<AuthorSelectedMessage>(this, HandleAuthorSelection);
            MessengerInstance.Register<BookSelectedMessage>(this, HandleBookChecked);
            MessengerInstance.Register<SeriesSelectedMessage>(this, HandleSeriesSelection);
            MessengerInstance.Register<CollectionSelectedMessage>(this, HandleCollectionSelection);
            MessengerInstance.Register<GoBackMessage>(this, _ => GoToPreviousViewer());
            MessengerInstance.Register<ResetPaneSelectionMessage>(this, _ =>
            {
                SelectedMainPaneItem = MainPaneItems[0];
                PaneSelectionChanged();
                HandleBookChecked(null);
            });
            MessengerInstance.Register<RefreshSidePaneCollectionsMessage>(this, CollectionsRefreshHandler);

            MainPaneItems = new ObservableCollection<PaneMainItem>
            {
                new PaneMainItem("All", "All Books", null),
                new PaneMainItem("Favorite", "Favorite Books", new FilterParameters { Favorite = true }),
                new PaneMainItem("Authors", "Authors", null),
                new PaneMainItem("Series", "Series", null)
            };
            SelectedMainPaneItem = MainPaneItems[0];
            PaneSelectionChanged();
            MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
        }

        public ObservableCollection<UserCollection> Collections { get; set; }

        public async void CollectionsRefreshHandler(RefreshSidePaneCollectionsMessage msg)
        {
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var collections = await uow.CollectionRepository.GetAllAsync(new QueryParameters
            {
                SortBy = new()
                {
                    PropertyName = "Tag"
                }
            });
            Collections = new ObservableCollection<UserCollection>(collections);
            RaisePropertyChanged(() => Collections);
        }

        public IViewer CurrentViewer => currentViewer;

        private async void SetCurrentViewer(IViewer value)
        {
            CurrentViewer?.Dispose();
            value.Back = history.Count > 0 ? GoToPreviousViewer : null;
            Set(() => CurrentViewer, ref currentViewer, value);
            await Task.Delay(10);
            CurrentViewer.Refresh();
        }

        public bool IsBackEnabled => history.Count > 0;

        public ObservableCollection<PaneMainItem> MainPaneItems { get; set; }

        public ICommand PaneSelectionChangedCommand => new RelayCommand(PaneSelectionChanged);

        public UserCollection SelectedCollection
        {
            get => selectedCollection;

            set
            {
                Set(() => SelectedCollection, ref selectedCollection, value);

                if (value == null)
                {
                    return;
                }

                history.Clear();

                var filter = new FilterParameters
                {
                    CollectionId = selectedCollection.Id
                };

                var newViewer = new BookViewerViewModel(filter)
                {
                    Caption = $"Collection {selectedCollection.Tag}"
                };

                SetCurrentViewer(newViewer);
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
            if (Selector.Instance.Count > 0 && !MainPaneItems.Contains(selectedMainItem))
            {
                MainPaneItems.Add(selectedMainItem);
            }
            else if (Selector.Instance.Count == 0)
            {
                _ = MainPaneItems.Remove(selectedMainItem);
            }
        }

        private void HandleCollectionSelection(CollectionSelectedMessage message)
        {
            SelectedCollection = Collections.FirstOrDefault(c => c.Id == message.CollectionId);
        }


        private void GoToPreviousViewer()
        {
            if (!IsBackEnabled)
            {
                return;
            }

            SetCurrentViewer(history.Pop()());
        }

        private void HandleAuthorSelection(AuthorSelectedMessage obj)
        {
            var viewerCaption = $"Books by {obj.Author.Name}";
            if (viewerCaption == CurrentViewer.Caption)
            {
                return;
            }

            history.Push(CurrentViewer.GetCloneFunction());

            var filter = new FilterParameters
            {
                AuthorId = obj.Author.Id
            };

            var newViewer = new BookViewerViewModel(filter)
            {
                Caption = viewerCaption,
                Back = GoToPreviousViewer
            };

            SetCurrentViewer(newViewer);
        }

        private void HandleSeriesSelection(SeriesSelectedMessage obj)
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
            history.Push(CurrentViewer.GetCloneFunction());

            SetCurrentViewer(new BookViewerViewModel(filter) { Caption = viewerCaption, Back = GoToPreviousViewer });
        }

        private void PaneSelectionChanged()
        {
            if (SelectedMainPaneItem == null)
            {
                return;
            }

            history.Clear();

            var filter = SelectedMainPaneItem.Filter != null
                ? SelectedMainPaneItem.Filter with { }
                : new FilterParameters();


            if (SelectedMainPaneItem.PaneCaption == "Authors")
            {
                SetCurrentViewer(new AuthorViewerViewModel() { Caption = SelectedMainPaneItem.PaneCaption });
            }
            else if ( SelectedMainPaneItem.PaneCaption == "Series")
            {
                SetCurrentViewer(new SeriesViewerViewModel() { Caption = SelectedMainPaneItem.PaneCaption });
            }
            else
            {
                SetCurrentViewer(new BookViewerViewModel(filter) { Caption = SelectedMainPaneItem.ViewerCaption });
            }
        }
    }
}
