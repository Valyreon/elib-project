using DataLayer;
using Domain;
using EbookTools;
using ElibWpf.BindingItems;
using ElibWpf.Extensions;
using ElibWpf.Messages;
using ElibWpf.ViewModels.Dialogs;
using ElibWpf.Views.Dialogs;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using Models;
using Models.Observables;
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
        private string caption = "Books";
        private IViewer currentViewer;
        private UserCollection selectedCollection;
        private PaneMainItem selectedMainPaneItem;
        private bool isInSearchResults = false;
        private readonly Selector Selector;

        private readonly PaneMainItem selectedMainItem;
        private bool isSelectedMainAdded = false;

        public BooksTabViewModel()
        {
            Selector = new Selector();
            selectedMainItem = new PaneMainItem("Selected", PackIconFontAwesomeKind.CheckDoubleSolid, "Selected Books", (Book x) => Selector.SelectedIds.Contains(x.Id), true);

            MessengerInstance.Register<AuthorSelectedMessage>(this, this.HandleAuthorSelection);
            MessengerInstance.Register<BookSelectedMessage>(this, this.HandleBookChecked);
            MessengerInstance.Register<SeriesSelectedMessage>(this, this.HandleSeriesSelection);
            MessengerInstance.Register<CollectionSelectedMessage>(this, this.HandleCollectionSelection);
            MessengerInstance.Register<GoBackMessage>(this, x => this.GoToPreviousViewer());
            MessengerInstance.Register<ResetPaneSelectionMessage>(this, x => { SelectedMainPaneItem = MainPaneItems[0]; PaneSelectionChanged(); });
            MessengerInstance.Register<RefreshSidePaneCollectionsMessage>(this, x => { RaisePropertyChanged(() => Collections); });


            viewerHistory.AddHandlerOnStackChange((object sender, NotifyCollectionChangedEventArgs e) => RaisePropertyChanged(() => IsBackEnabled));

            MainPaneItems = new ObservableCollection<PaneMainItem>
            {
                new PaneMainItem("All", PackIconBoxIconsKind.SolidBook, "All Books", (Book x) => true),
                new PaneMainItem("Favorite", PackIconFontAwesomeKind.StarSolid, "Favorite Books", (Book x) => x.IsFavorite),
                new PaneMainItem("Read", PackIconFontAwesomeKind.CheckSolid, "Read Books", (Book x) => x.IsRead),
                new PaneMainItem("Not Read", PackIconJamIconsKind.CloseCircle, "Not Read Books", (Book x) => !x.IsRead)
            };
            SelectedMainPaneItem = MainPaneItems[0];
            PaneSelectionChanged();
            SearchOptions = ApplicationSettings.GetInstance().SearchOptions;
        }

        private void HandleBookChecked(BookSelectedMessage obj)
        {
            if (Selector.Count > 0 && !isSelectedMainAdded)
            {
                MainPaneItems.Add(selectedMainItem);
                isSelectedMainAdded = true;
            }
            else if (Selector.Count == 0)
            {
                MainPaneItems.Remove(selectedMainItem); isSelectedMainAdded = false;
            }
        }

        private void HandleCollectionSelection(CollectionSelectedMessage message)
        {
            SelectedCollection = Collections.Where(c => c.Id == message.Collection.Id).FirstOrDefault();
        }

        public ICommand BackCommand { get => new RelayCommand(this.GoToPreviousViewer); }

        public ICommand SearchCommand { get => new RelayCommand<string>(this.ProcessSearchInput); }

        public ICommand AddBookCommand { get => new RelayCommand(this.ProcessAddBook); }

        public ICommand ExportSelectedBooksCommand { get => new RelayCommand(this.HandleExport); }

        private async void HandleExport()
        {
            var dialog = new ExportOptionsDialog();
            using ElibContext Database = ApplicationSettings.CreateContext();
            dialog.DataContext = new ExportOptionsDialogViewModel(await Selector.GetSelectedBooks(Database), dialog);
            await DialogCoordinator.Instance.ShowMetroDialogAsync(App.Current.MainWindow.DataContext, dialog);
        }

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
                var controller = await DialogCoordinator.Instance.ShowProgressAsync(App.Current.MainWindow.DataContext, "Please wait...", "");
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
                        var content = File.ReadAllBytes(dlg.FileNames[i]);
                        booksToAdd.Add(new Book
                        {
                            UserCollections = new List<UserCollection>(),
                            File = new EFile
                            {
                                Format = Path.GetExtension(dlg.FileNames[i]),
                                Signature = Signer.ComputeHash(content),
                                RawFile = new RawFile { RawContent = content }
                            },
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
                token = token.ToLower();
                if (!isInSearchResults)
                {
                    viewerHistory.Push(CurrentViewer);
                    isInSearchResults = true;
                }

                Func<Book, bool> condition = (Book x) => ((SearchOptions.SearchByName ? x.Title.ToLower().Contains(token) : false) ||
                                                        (SearchOptions.SearchByAuthor ? x.Authors.Where(a => a.Name.ToLower().Contains(token)).Any() : false) ||
                                                        (SearchOptions.SearchBySeries ? x.Series != null && x.Series.Name.ToLower().Contains(token) : false)) &&
                                                        viewerHistory.Peek().DefaultCondition(x);

                using ElibContext dbcontext = ApplicationSettings.CreateContext();
                int temp = await Task.Run(() => dbcontext.Books.Include("Series").Include("Authors").Where(condition).Count());

                if (temp > 0)
                    CurrentViewer = new BookViewerViewModel($"Search results for '{token}' in " + viewerHistory.Peek().Caption, condition, Selector);
                else
                    MessengerInstance.Send(new ShowDialogMessage("No matches", "No books found matching the search conditions."));
            }
        }

        public string Caption
        {
            get => caption;
            set => Set(ref caption, value);
        }

        public List<UserCollection> Collections
        {
            get
            {
                using ElibContext database = ApplicationSettings.CreateContext();
                return database.UserCollections.ToList();
            }
        }

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
                    CurrentViewer = new BookViewerViewModel($"Collection {selectedCollection.Tag}", (Book x) => x.UserCollections.Where(c => c.Id == SelectedCollection.Id).Count() > 0, Selector);
                }
            }
        }

        public PaneMainItem SelectedMainPaneItem
        {
            get => selectedMainPaneItem;
            set => Set("SelectedMainPaneItem", ref selectedMainPaneItem, value);
        }

        public bool IsSelectedBooksViewer
        {
            get => SelectedMainPaneItem == selectedMainItem;
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
                Func<Book, bool> condition = (Book x) =>
                {
                    var bookAuthorsIds = x.Authors.Select(a => a.Id);
                    foreach (ObservableAuthor selected in obj.Authors)
                    {
                        if (!bookAuthorsIds.Contains(selected.Id))
                        {
                            return false;
                        }
                    }
                    return true;
                };
                CurrentViewer = new BookViewerViewModel(viewerCaption, condition, Selector);
            }
        }

        private void HandleSeriesSelection(SeriesSelectedMessage obj)
        {
            string viewerCaption = $"{obj.Series.Name} Series";
            if (viewerCaption != CurrentViewer.Caption)
            {
                viewerHistory.Push(CurrentViewer);
                CurrentViewer = new BookViewerViewModel(viewerCaption, (Book x) => x.SeriesId.HasValue && x.SeriesId == obj.Series.Id, Selector);
            }
        }

        private void PaneSelectionChanged()
        {
            if (SelectedMainPaneItem != null)
            {
                RaisePropertyChanged(() => IsSelectedBooksViewer);
                viewerHistory.Clear();
                CurrentViewer = new BookViewerViewModel(SelectedMainPaneItem.ViewerCaption, SelectedMainPaneItem.Condition, Selector, SelectedMainPaneItem.IsSelectedBooksPane);
            }
        }

        private void RefreshCurrent()
        {
            if (SelectedCollection == null && SelectedMainPaneItem == null) SelectedMainPaneItem = MainPaneItems[0];
            CurrentViewer = CurrentViewer.Clone() as IViewer;
        }
    }
}