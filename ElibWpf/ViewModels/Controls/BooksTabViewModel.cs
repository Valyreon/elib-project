using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
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
using Application = System.Windows.Application;

namespace ElibWpf.ViewModels.Controls
{
    public class BooksTabViewModel : ViewModelBase, ITabViewModel
    {
        private readonly PaneMainItem selectedMainItem;
        private readonly Selector selector;
        private readonly ObservableStack<IViewer> viewerHistory = new ObservableStack<IViewer>();
        private string caption = "Books";
        private IViewer currentViewer;
        private bool isInSearchResults;
        private bool isSelectedMainAdded;

        private SearchOptions searchOptions;
        private UserCollection selectedCollection;
        private PaneMainItem selectedMainPaneItem;

        public BooksTabViewModel()
        {
            this.selector = new Selector();
            this.selectedMainItem = new PaneMainItem("Selected", PackIconFontAwesomeKind.CheckDoubleSolid, "Selected Books",
                x => this.selector.SelectedIds.Contains(x.Id), true);

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


            this.viewerHistory.AddHandlerOnStackChange((sender, e) => this.RaisePropertyChanged(() => this.IsBackEnabled));

            this.MainPaneItems = new ObservableCollection<PaneMainItem>
            {
                new PaneMainItem("All", PackIconBoxIconsKind.SolidBook, "All Books", x => true),
                new PaneMainItem("Favorite", PackIconFontAwesomeKind.StarSolid, "Favorite Books", x => x.IsFavorite),
                new PaneMainItem("Read", PackIconFontAwesomeKind.CheckSolid, "Read Books", x => x.IsRead),
                new PaneMainItem("Not Read", PackIconJamIconsKind.CloseCircle, "Not Read Books", x => !x.IsRead)
            };
            this.SelectedMainPaneItem = this.MainPaneItems[0];
            this.PaneSelectionChanged();
            this.SearchOptions = ApplicationSettings.GetInstance().SearchOptions;
        }

        public ICommand AddBookCommand => new RelayCommand(this.ProcessAddBook);

        public ICommand BackCommand => new RelayCommand(this.GoToPreviousViewer);

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
            get => this.currentViewer;
            set => this.Set(ref this.currentViewer, value);
        }

        public ICommand ExportSelectedBooksCommand => new RelayCommand(this.HandleExport);

        public bool IsBackEnabled => this.viewerHistory.Count > 0;

        public bool IsSelectedBooksViewer => this.SelectedMainPaneItem == this.selectedMainItem;

        public ObservableCollection<PaneMainItem> MainPaneItems { get; set; }

        public ICommand PaneSelectionChangedCommand => new RelayCommand(this.PaneSelectionChanged);

        public ICommand RefreshCommand => new RelayCommand(this.RefreshCurrent);

        public ICommand SearchCheckboxChangedCommand => new RelayCommand(this.ProcessSearchCheckboxChanged);

        public ICommand SearchCommand => new RelayCommand<string>(this.ProcessSearchInput);

        public SearchOptions SearchOptions
        {
            get => this.searchOptions;
            set => this.Set(() => this.SearchOptions, ref this.searchOptions, value);
        }

        public UserCollection SelectedCollection
        {
            get => this.selectedCollection;

            set
            {
                this.Set(ref this.selectedCollection, value);
                if (this.selectedCollection != null)
                {
                    this.viewerHistory.Clear();
                    this.CurrentViewer = new BookViewerViewModel($"Collection {this.selectedCollection.Tag}",
                        x => x.UserCollections.Any(c => c.Id == this.SelectedCollection.Id), this.selector);
                }
            }
        }

        public PaneMainItem SelectedMainPaneItem
        {
            get => this.selectedMainPaneItem;
            set => this.Set("SelectedMainPaneItem", ref this.selectedMainPaneItem, value);
        }

        public string Caption
        {
            get => this.caption;
            set => this.Set(ref this.caption, value);
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
            this.SelectedCollection = this.Collections.FirstOrDefault(c => c.Id == message.Collection.Id);
        }

        private async void HandleExport()
        {
            ExportOptionsDialog dialog = new ExportOptionsDialog();
            using ElibContext database = ApplicationSettings.CreateContext();
            dialog.DataContext = new ExportOptionsDialogViewModel(await this.selector.GetSelectedBooks(database), dialog);
            await DialogCoordinator.Instance.ShowMetroDialogAsync(Application.Current.MainWindow.DataContext, dialog);
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
            DialogResult result = dlg.ShowDialog();
            if (result == DialogResult.OK && dlg.FileNames.Any())
            {
                var booksToAdd = new List<Book>();
                ProgressDialogController controller =
                    await DialogCoordinator.Instance.ShowProgressAsync(Application.Current.MainWindow.DataContext,
                        "Please wait...", "");
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
                                RawFile = new RawFile {RawContent = content}
                            },
                            Authors = new List<Author>()
                        });
                    }
                }

                await controller.CloseAsync();
                this.MessengerInstance.Send(new OpenAddBooksFormMessage(booksToAdd));
            }
        }

        private async void ProcessSearchInput(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                token = token.ToLower();
                if (!this.isInSearchResults)
                {
                    this.viewerHistory.Push(this.CurrentViewer);
                    this.isInSearchResults = true;
                }

                Func<Book, bool> condition = x => (this.SearchOptions.SearchByName && x.Title.ToLower().Contains(token) ||
                                                   this.SearchOptions.SearchByAuthor &&
                                                   x.Authors.Any(a => a.Name.ToLower().Contains(token)) || this.SearchOptions.SearchBySeries &&
                                                   x.Series != null &&
                                                   x.Series.Name.ToLower().Contains(token)) && this.viewerHistory.Peek().DefaultCondition(x);

                using ElibContext context = ApplicationSettings.CreateContext();
                int temp = await Task.Run(() =>
                    context.Books.Include("Series").Include("Authors").Where(condition).Count());

                if (temp > 0)
                {
                    this.CurrentViewer = new BookViewerViewModel(
                        $"Search results for '{token}' in " + this.viewerHistory.Peek().Caption, condition, this.selector);
                }
                else
                {
                    this.MessengerInstance.Send(new ShowDialogMessage("No matches",
                        "No books found matching the search conditions."));
                }
            }
        }

        private void ProcessSearchCheckboxChanged()
        {
            if (!this.SearchOptions.SearchByName && !this.SearchOptions.SearchByAuthor && !this.SearchOptions.SearchBySeries)
            {
                this.SearchOptions = new SearchOptions();
            }

            ApplicationSettings.GetInstance().SearchOptions.SearchByName = this.SearchOptions.SearchByName;
            ApplicationSettings.GetInstance().SearchOptions.SearchByAuthor = this.SearchOptions.SearchByAuthor;
            ApplicationSettings.GetInstance().SearchOptions.SearchBySeries = this.SearchOptions.SearchBySeries;
        }

        private void GoToPreviousViewer()
        {
            if (!this.IsBackEnabled)
            {
                return;
            }

            this.isInSearchResults = false;
            this.CurrentViewer = this.viewerHistory.Pop();
        }

        private void HandleAuthorSelection(AuthorSelectedMessage obj)
        {
            string viewerCaption = $"Books by {obj.Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j)}";
            if (viewerCaption == this.CurrentViewer.Caption)
            {
                return;
            }

            this.viewerHistory.Push(this.CurrentViewer);
            Func<Book, bool> condition = x =>
            {
                var bookAuthorsIds = x.Authors.Select(a => a.Id);
                return obj.Authors.All(selected => bookAuthorsIds.Contains(selected.Id));
            };
            this.CurrentViewer = new BookViewerViewModel(viewerCaption, condition, this.selector);
        }

        private void HandleSeriesSelection(SeriesSelectedMessage obj)
        {
            string viewerCaption = $"{obj.Series.Name} Series";
            if (viewerCaption == this.CurrentViewer.Caption)
            {
                return;
            }

            this.viewerHistory.Push(this.CurrentViewer);
            this.CurrentViewer = new BookViewerViewModel(viewerCaption,
                x => x.SeriesId.HasValue && x.SeriesId == obj.Series.Id, this.selector);
        }

        private void PaneSelectionChanged()
        {
            if (this.SelectedMainPaneItem == null)
            {
                return;
            }

            this.RaisePropertyChanged(() => this.IsSelectedBooksViewer);
            this.viewerHistory.Clear();
            this.CurrentViewer = new BookViewerViewModel(this.SelectedMainPaneItem.ViewerCaption, this.SelectedMainPaneItem.Condition, this.selector,
                this.SelectedMainPaneItem.IsSelectedBooksPane);
        }

        private void RefreshCurrent()
        {
            if (this.SelectedCollection == null && this.SelectedMainPaneItem == null)
            {
                this.SelectedMainPaneItem = this.MainPaneItems[0];
            }

            this.CurrentViewer = this.CurrentViewer.Clone() as IViewer;
        }
    }
}