using Domain;
using EbookTools;
using ElibWpf.BindingItems;
using ElibWpf.DataStructures;
using ElibWpf.Extensions;
using ElibWpf.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using Models;
using Models.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Controls
{
    public class BooksTabViewModel : ViewModelBase, ITabViewModel
    {
        private readonly ObservableStack<IViewer> viewerHistory = new ObservableStack<IViewer>();
        private readonly IDialogCoordinator dialogCoordinator;
        private string caption = "Books";
        private IViewer currentViewer;
        private UserCollection selectedCollection;
        private PaneMainItem selectedMainPaneItem;
        private bool isInSearchResults = false;

        private readonly PaneMainItem selectedMainItem = new PaneMainItem("Selected", PackIconFontAwesomeKind.CheckDoubleSolid, "Selected Books", (Book x) => App.Selector.SelectedIds.Contains(x.Id), true);
        private bool isSelectedMainAdded = false;

        public BooksTabViewModel(IDialogCoordinator dialogCoordinator)
        {
            MessengerInstance.Register<AuthorSelectedMessage>(this, this.HandleAuthorSelection);
            MessengerInstance.Register(this, (BookSelectedMessage m) => { if (App.Selector.Count > 0 && !isSelectedMainAdded) { MainPaneItems.Add(selectedMainItem); isSelectedMainAdded = true; } else if (App.Selector.Count == 0) { MainPaneItems.Remove(selectedMainItem); isSelectedMainAdded = false; } });
            MessengerInstance.Register<SeriesSelectedMessage>(this, this.HandleSeriesSelection);
            MessengerInstance.Register<CollectionSelectedMessage>(this, this.HandleCollectionSelection);
            MessengerInstance.Register<GoBackMessage>(this, x => this.GoToPreviousViewer());
            MessengerInstance.Register<ResetPaneSelectionMessage>(this, x => { SelectedMainPaneItem = MainPaneItems[0]; PaneSelectionChanged(); });
            MessengerInstance.Register<RefreshSidePaneCollectionsMessage>(this, async x => { this.Collections = await Task.Run(() => App.Database.UserCollections.ToList()); RaisePropertyChanged("Collections"); });

            viewerHistory.AddHandlerOnStackChange((object sender, NotifyCollectionChangedEventArgs e) => RaisePropertyChanged("IsBackEnabled"));

            MainPaneItems = new ObservableCollection<PaneMainItem>
            {
                new PaneMainItem("All", PackIconUniconsKind.Books, "All Books", (Book x) => true),
                new PaneMainItem("Favorite", PackIconFontAwesomeKind.StarSolid, "Favorite Books", (Book x) => x.IsFavorite),
                new PaneMainItem("Read", PackIconFontAwesomeKind.CheckSolid, "Read Books", (Book x) => x.IsRead),
                new PaneMainItem("Not Read", PackIconJamIconsKind.CloseCircle, "Not Read Books", (Book x) => !x.IsRead)
            };
            SelectedMainPaneItem = MainPaneItems[0];
            PaneSelectionChanged();
            SearchOptions = ApplicationSettings.GetInstance().SearchOptions;
            this.dialogCoordinator = dialogCoordinator;
        }

        private void HandleCollectionSelection(CollectionSelectedMessage message)
        {
            SelectedCollection = Collections.Where(c => c.Id == message.Collection.Id).FirstOrDefault();
        }

        public ICommand BackCommand { get => new RelayCommand(this.GoToPreviousViewer); }

        public ICommand SearchCommand { get => new RelayCommand<string>(this.ProcessSearchInput); }

        public ICommand AddBookCommand { get => new RelayCommand(this.ProcessAddBook); }

        private async void ProcessAddBook()
        {
            using OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Epub files|*.epub|Mobi files|*.mobi|All files|*.*",
                CheckFileExists = true,
                CheckPathExists = true,
                FilterIndex = 3,
                Multiselect = true
            };
            var result = dlg.ShowDialog();
            if (result == DialogResult.OK && dlg.FileNames.Any())
            {
                List<Book> booksToAdd = new List<Book>();
                var controller = await dialogCoordinator.ShowProgressAsync(App.Current.MainWindow.DataContext, "Please wait...", "");
                controller.Maximum = dlg.FileNames.Length;
                controller.Minimum = 1;
                for (int i = 0; i < dlg.FileNames.Length; i++)
                {
                    await Task.Run(() =>
                    {
                        controller.SetMessage($"Parsing book: {i + 1}");
                        controller.SetProgress(i + 1);
                    });
                    try
                    {
                        await Task.Run(() =>
                        {
                            ParsedBook pBook = EbookParserFactory.Create(dlg.FileNames[i]).Parse();
                            Book book = pBook.ToBook();
                            booksToAdd.Add(book);
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Log("BOOK_PARSE_ERROR", $"\nMESSAGE:{e.Message}\nSTACK:{e.StackTrace}");
                        booksToAdd.Add(new Book
                        {
                            UserCollections = new List<UserCollection>(),
                            Files = new List<EFile>() { new EFile() { Format = Path.GetExtension(dlg.FileNames[i]), RawContent = File.ReadAllBytes(dlg.FileNames[i]) } },
                            Authors = new List<Author>()
                        });
                    }
                }
                await controller.CloseAsync();
                MessengerInstance.Send(new OpenAddBooksFormMessage(booksToAdd)); // TODO: dont forget to add subscription in Window later
            }
        }

        private async void ProcessSearchInput(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                if (!isInSearchResults)
                {
                    viewerHistory.Push(CurrentViewer);
                    isInSearchResults = true;
                }
                Func<Book, bool> condition = (Book x) => ((SearchOptions.SearchByName ? x.Title.Contains(token) : false) ||
                                                        (SearchOptions.SearchByAuthor ? x.Authors.Where(a => a.Name.Contains(token)).Any() : false) ||
                                                        (SearchOptions.SearchBySeries ? x.Series != null && x.Series.Name.Contains(token) : false)) &&
                                                        viewerHistory.Peek().DefaultCondition(x);
                int temp = await Task.Run(() => App.Database.Books.Where(condition).Count());

                if (temp > 0)
                    CurrentViewer = new BookViewerViewModel($"Search results for '{token}' in " + viewerHistory.Peek().Caption, condition);
                else
                    MessengerInstance.Send(new ShowDialogMessage("No matches", "No books found matching the search conditions."));
            }
        }

        public string Caption
        {
            get => caption;
            set => Set(ref caption, value);
        }

        public List<UserCollection> Collections { get; set; } = App.Database.UserCollections.ToList();

        public IViewer CurrentViewer
        {
            get => currentViewer;
            set => Set(ref currentViewer, value);
        }

        public bool IsBackEnabled { get => viewerHistory.Count > 0; }

        public ObservableCollection<PaneMainItem> MainPaneItems { get; set; }

        private SearchOptions searchOptions;

        public SearchOptions SearchOptions
        {
            get => searchOptions;
            set => Set(() => SearchOptions, ref searchOptions, value);
        }

        public ICommand PaneSelectionChangedCommand { get => new RelayCommand(this.PaneSelectionChanged); }

        public ICommand RefreshCommand { get => new RelayCommand(this.RefreshCurrent); }

        public ICommand SearchCheckboxChangedCommand { get => new RelayCommand(this.ProcessSearchCheckboxChanged); }

        private void ProcessSearchCheckboxChanged()
        {
            if (!SearchOptions.SearchByName && !SearchOptions.SearchByAuthor && !SearchOptions.SearchBySeries)
            {
                SearchOptions = new SearchOptions();
            }

            ApplicationSettings.GetInstance().SearchOptions.SearchByName = SearchOptions.SearchByName;
            ApplicationSettings.GetInstance().SearchOptions.SearchByAuthor = SearchOptions.SearchByAuthor;
            ApplicationSettings.GetInstance().SearchOptions.SearchBySeries = SearchOptions.SearchBySeries;
        }

        public UserCollection SelectedCollection
        {
            get => selectedCollection;

            set
            {
                Set(ref selectedCollection, value);
                if (selectedCollection != null)
                {
                    viewerHistory.Clear();
                    CurrentViewer = new BookViewerViewModel($"Collection {selectedCollection.Tag}", (Book x) => x.UserCollections.Where(c => c.Id == SelectedCollection.Id).Count() > 0);
                }
            }
        }

        public PaneMainItem SelectedMainPaneItem
        {
            get => selectedMainPaneItem;
            set => Set("SelectedMainPaneItem", ref selectedMainPaneItem, value);
        }

        private void GoToPreviousViewer()
        {
            if (IsBackEnabled)
            {
                isInSearchResults = false;
                CurrentViewer = viewerHistory.Pop();
            }
        }

        private void HandleAuthorSelection(AuthorSelectedMessage obj)
        {
            string viewerCaption = $"Books by {obj.Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j)}";
            if (viewerCaption != CurrentViewer.Caption)
            {
                viewerHistory.Push(CurrentViewer);
                Func<Book, bool> selector = (Book x) =>
                {
                    var bookAuthorsIds = x.Authors.Select(a => a.Id);
                    foreach (Author selected in obj.Authors)
                    {
                        if (!bookAuthorsIds.Contains(selected.Id))
                        {
                            return false;
                        }
                    }
                    return true;
                };
                CurrentViewer = new BookViewerViewModel(viewerCaption, selector);
            }
        }

        private void HandleSeriesSelection(SeriesSelectedMessage obj)
        {
            string viewerCaption = $"{obj.Series.Name} Series";
            if (viewerCaption != CurrentViewer.Caption)
            {
                viewerHistory.Push(CurrentViewer);
                CurrentViewer = new BookViewerViewModel(viewerCaption, (Book x) => x.SeriesId.HasValue && x.SeriesId == obj.Series.Id);
            }
        }

        private void PaneSelectionChanged()
        {
            if (SelectedMainPaneItem != null)
            {
                viewerHistory.Clear();
                CurrentViewer = new BookViewerViewModel(SelectedMainPaneItem.ViewerCaption, SelectedMainPaneItem.Condition, SelectedMainPaneItem.IsSelectedBooksPane);
            }
        }

        private void RefreshCurrent()
        {
            if (SelectedCollection == null && SelectedMainPaneItem == null) SelectedCollection = Collections[0];
            CurrentViewer = CurrentViewer.Clone() as IViewer;
        }
    }
}