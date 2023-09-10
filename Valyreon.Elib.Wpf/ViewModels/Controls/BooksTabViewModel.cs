using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Valyreon.Elib.BookDataAPI.GoogleBooks;
using Valyreon.Elib.DataLayer;
using Valyreon.Elib.DataLayer.Filters;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;
using Valyreon.Elib.EBookTools.Epub;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.BindingItems;
using Valyreon.Elib.Wpf.CustomDataStructures;
using Valyreon.Elib.Wpf.Extensions;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.Services;
using Valyreon.Elib.Wpf.ViewModels.Flyouts;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class BooksTabViewModel : ViewModelBase, ITabViewModel
    {
        private readonly PaneMainItem selectedMainItem;
        private readonly ViewerHistory history = new();
        private string caption = "Books";
        private IViewer currentViewer;
        private readonly Selector selector = new Selector();
        private readonly ApplicationProperties applicationProperties;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private UserCollection selectedCollection;
        private PaneMainItem selectedMainPaneItem;

        public BooksTabViewModel(ApplicationProperties applicationProperties, IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.applicationProperties = applicationProperties;
            this.unitOfWorkFactory = unitOfWorkFactory;
            selectedMainItem = new PaneMainItem("Selected", "Selected Books", new BookFilter { Selected = true });

            MessengerInstance.Register<AuthorSelectedMessage>(this, HandleAuthorSelection);
            MessengerInstance.Register<SeriesSelectedMessage>(this, HandleSeriesSelection);
            MessengerInstance.Register<CollectionSelectedMessage>(this, HandleCollectionSelection);
            MessengerInstance.Register<GoBackMessage>(this, _ => GoToPreviousViewer());
            MessengerInstance.Register<ResetPaneSelectionMessage>(this, _ => SelectedMainPaneItem = MainPaneItems[0]);
            MessengerInstance.Register<RefreshSidePaneCollectionsMessage>(this, CollectionsRefreshHandler);

            MainPaneItems = new ObservableCollection<PaneMainItem>
            {
                new PaneMainItem("All", "All Books", null),
                new PaneMainItem("Favorite", "Favorite Books", new BookFilter { Favorite = true }),
                new PaneMainItem("Authors", "Authors", null),
                new PaneMainItem("Series", "Series", null)
            };
            SelectedMainPaneItem = MainPaneItems[0];
            CollectionsRefreshHandler(null);

            selector.SelectionChanged += HandleSelectionChanged;
        }

        private void HandleSelectionChanged()
        {
            if (selector.Count > 0 && !MainPaneItems.Contains(selectedMainItem))
            {
                MainPaneItems.Add(selectedMainItem);
            }
            else if (selector.Count == 0)
            {
                _ = MainPaneItems.Remove(selectedMainItem);
            }
        }

        public ObservableCollection<UserCollection> Collections { get; set; }

        public async void CollectionsRefreshHandler(RefreshSidePaneCollectionsMessage msg)
        {
            using var uow = await unitOfWorkFactory.CreateAsync();
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

        private void SetCurrentViewer(IViewer value)
        {
            UnitOfWork.ClearCache();
            CurrentViewer?.Dispose();
            value.Back = history.Count > 0 ? GoToPreviousViewer : null;
            Set(() => CurrentViewer, ref currentViewer, value);
            CurrentViewer.Refresh();
        }

        public bool IsBackEnabled => history.Count > 0;

        public ObservableCollection<PaneMainItem> MainPaneItems { get; set; }

        public UserCollection SelectedCollection
        {
            get => selectedCollection;

            set => Set(() => SelectedCollection, ref selectedCollection, value);
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

        private void HandleCollectionSelection(CollectionSelectedMessage message)
        {
            SelectedCollection = Collections.FirstOrDefault(c => c.Id == message.CollectionId);
            HandleCollectionSelectionChanged();
        }

        public ICommand CollectionSelectionChangedCommand => new RelayCommand(HandleCollectionSelectionChanged);

        private void HandleCollectionSelectionChanged()
        {
            if (selectedCollection == null)
            {
                return;
            }

            history.Clear();

            var filter = new BookFilter
            {
                CollectionId = selectedCollection.Id
            };

            var newViewer = new BookViewerViewModel(filter, selector, applicationProperties, unitOfWorkFactory)
            {
                Caption = $"Collection {selectedCollection.Tag}"
            };

            SetCurrentViewer(newViewer);
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

            BookFilter filter = null;

            if (applicationProperties.RememberFilterInNextView && CurrentViewer.GetFilter() is BookFilter bFilter)
            {
                filter = bFilter with { AuthorId = obj.Author.Id };
            }
            else
            {
                filter = new BookFilter { AuthorId = obj.Author.Id };
            }

            var newViewer = new BookViewerViewModel(filter, selector, applicationProperties, unitOfWorkFactory)
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

            BookFilter filter = null;

            if (applicationProperties.RememberFilterInNextView && CurrentViewer.GetFilter() is BookFilter bFilter)
            {
                filter = bFilter with { SeriesId = obj.Series.Id };
            }
            else
            {
                filter = new BookFilter { SeriesId = obj.Series.Id };
            }

            history.Push(CurrentViewer.GetCloneFunction());

            SetCurrentViewer(new BookViewerViewModel(filter, selector, applicationProperties, unitOfWorkFactory) { Caption = viewerCaption, Back = GoToPreviousViewer });
        }

        public ICommand PaneSelectionChangedCommand => new RelayCommand(PaneSelectionChanged);

        public ICommand LoadedCommand => new RelayCommand(HandleLoaded);

        private void HandleLoaded()
        {
            PaneSelectionChanged();

            if (!applicationProperties.AutomaticallyImportWithFoundISBN)
            {
                return;
            }

            _ = Task.Run(async () =>
            {
                IReadOnlyList<string> foundBooks = null;
                var importService = new ImportService(unitOfWorkFactory, applicationProperties);
                foundBooks = await importService.GetNotImportedBookPathsAsync();

                //MessengerInstance.Send(new ShowNotificationMessage($"Found {foundBooks.Count} books to be imported."));
                var counter = 0;
                var withISBN = 0;
                var nullReturned = 0;
                var excHappened = 0;

                var epubCount = foundBooks.Count(b => b.ToLowerInvariant().EndsWith(".epub"));
                var mobiCount = foundBooks.Count(b => b.ToLowerInvariant().EndsWith(".mobi"));
                var pdfCount = foundBooks.Count(b => b.ToLowerInvariant().EndsWith(".pdf"));

                foreach (var book in foundBooks.Where(b => b.ToLowerInvariant().EndsWith(".epub")))
                {
                    var parser = new VersOneEpubParser(book);
                    try
                    {
                        var parsedBook = parser.Parse();

                        if (string.IsNullOrWhiteSpace(parsedBook.Isbn))
                        {
                            continue;
                        }

                        withISBN++;
                        var client = new GoogleBooksClient();
                        var bData = await client.GetByIsbnAsync(parsedBook.Isbn);

                        if (bData == null)
                        {
                            nullReturned++;
                            continue;
                        }

                        //File.WriteAllText(@"C:\Users\Luka\Desktop\log.txt", JsonConvert.SerializeObject(result, Formatting.Indented));
                        //parsedBook.Description = olBook.Data.Description;

                        parsedBook.Title = bData.Title;
                        parsedBook.Authors = bData.Authors.ToList();
                        parsedBook.Cover = bData.Cover ?? parsedBook.Cover;
                        parsedBook.Description = bData.Description;
                        parsedBook.Publisher = bData.Publisher;

                        var bookForImport = await parsedBook.ToBookAsync(unitOfWorkFactory);

                        await importService.ImportBookAsync(bookForImport);
                        counter++;
                    }
                    catch (Exception ex)
                    {
                        excHappened++;
                        var m = ex.Message;
                    }
                }

                if (counter > 0)
                {
                    MessengerInstance.Send(new ShowNotificationMessage($"Successfully imported {counter} books using Google Books."));
                }
            });
        }

        private void PaneSelectionChanged()
        {
            if (selectedMainPaneItem == null)
            {
                return;
            }

            history.Clear();

            var filter = selectedMainPaneItem.Filter != null
                ? selectedMainPaneItem.Filter with { }
                : new BookFilter();

            if (selectedMainPaneItem.PaneCaption == "Authors")
            {
                SetCurrentViewer(new AuthorViewerViewModel(new Filter(), unitOfWorkFactory) { Caption = selectedMainPaneItem.PaneCaption });
            }
            else if (selectedMainPaneItem.PaneCaption == "Series")
            {
                SetCurrentViewer(new SeriesViewerViewModel(new Filter(), unitOfWorkFactory) { Caption = selectedMainPaneItem.PaneCaption });
            }
            else
            {
                SetCurrentViewer(new BookViewerViewModel(filter, selector, applicationProperties, unitOfWorkFactory) { Caption = selectedMainPaneItem.ViewerCaption });
            }
        }

        public ICommand ImportCommand => new RelayCommand(HandleImport);

        private void HandleImport()
        {
            _ = Task.Run(async () =>
            {
                var importer = new ImportService(unitOfWorkFactory, applicationProperties);

                MessengerInstance.Send(new SetGlobalLoaderMessage(true));
                var newBookPaths = await importer.GetNotImportedBookPathsAsync();

                if (!newBookPaths.Any())
                {
                    return;
                }

                var importFlyout = new AddNewBooksViewModel(newBookPaths, unitOfWorkFactory);
                MessengerInstance.Send(new SetGlobalLoaderMessage(false));

                Application.Current.Dispatcher.Invoke(() => MessengerInstance.Send(new OpenFlyoutMessage(importFlyout)));
            });
        }
    }
}
