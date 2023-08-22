using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.IconPacks;
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
        private readonly Selector selector;
        private readonly ViewerHistory history = new ViewerHistory();
        private string caption = "Books";
        private IViewer currentViewer;
        private bool isSelectedMainAdded;

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
            MessengerInstance.Register<GoBackMessage>(this, _ => GoToPreviousViewer());
            MessengerInstance.Register<ResetPaneSelectionMessage>(this, _ =>
            {
                SelectedMainPaneItem = MainPaneItems[0];
                PaneSelectionChanged();
            });
            MessengerInstance.Register<RefreshSidePaneCollectionsMessage>(this, CollectionsRefreshHandler);

            MainPaneItems = new ObservableCollection<PaneMainItem>
            {
                new PaneMainItem("All", PackIconBoxIconsKind.SolidBook, "All Books", null),
                new PaneMainItem("Favorite", PackIconFontAwesomeKind.StarSolid, "Favorite Books", new FilterParameters { Favorite = true }),
                new PaneMainItem("Authors", PackIconFontAwesomeKind.PersonBoothSolid, "Authors", null),
                new PaneMainItem("Series", PackIconFontAwesomeKind.LinkSolid, "Series", null)
            };
            SelectedMainPaneItem = MainPaneItems[0];
            PaneSelectionChanged();
            MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
        }

        public ObservableCollection<UserCollection> Collections { get; set; }

        public async void CollectionsRefreshHandler(RefreshSidePaneCollectionsMessage msg)
        {
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            Collections = new ObservableCollection<UserCollection>(await uow.CollectionRepository.GetAllAsync());
            RaisePropertyChanged(() => Collections);
        }

        public IViewer CurrentViewer => currentViewer;

        private async void SetCurrentViewer(IViewer value)
        {
            Set(() => CurrentViewer, ref currentViewer, value);
            await Task.Delay(20);
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

                var newViewer = new BookViewerViewModel(filter, selector)
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


        private void GoToPreviousViewer()
        {
            if (!IsBackEnabled)
            {
                return;
            }

            SetCurrentViewer(history.Pop());
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

            var newViewer = new BookViewerViewModel(filter, selector)
            {
                Caption = viewerCaption,
                Back = GoToPreviousViewer
            };
            SetCurrentViewer(newViewer);

            history.Push(temp);
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

            var temp = CurrentViewer;

            SetCurrentViewer(new BookViewerViewModel(filter, selector) { Caption = viewerCaption, Back = GoToPreviousViewer });

            history.Push(temp);
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
                SetCurrentViewer(new BookViewerViewModel(filter, selector) { Caption = SelectedMainPaneItem.ViewerCaption });
            }
        }
    }
}
