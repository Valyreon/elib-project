using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
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
        private readonly ObservableStack<ViewerState> viewerHistory = new ObservableStack<ViewerState>();
        private string caption = "Books";
        private IViewer currentViewer;
        private bool isInSearchResults;
        private bool isSelectedMainAdded;

        private SearchOptions searchOptions;
        private ObservableUserCollection selectedCollection;
        private PaneMainItem selectedMainPaneItem;
        private FilterOptions filterOptions;

        public BooksTabViewModel()
        {
            this.selector = new Selector();
            this.selectedMainItem = new PaneMainItem("Selected", PackIconFontAwesomeKind.CheckDoubleSolid, "Selected Books", new Filter { Selected = true });

            this.MessengerInstance.Register<AuthorSelectedMessage>(this, this.HandleAuthorSelection);
            this.MessengerInstance.Register<BookSelectedMessage>(this, this.HandleBookChecked);
            this.MessengerInstance.Register<SeriesSelectedMessage>(this, this.HandleSeriesSelection);
            this.MessengerInstance.Register<CollectionSelectedMessage>(this, this.HandleCollectionSelection);
            this.MessengerInstance.Register<GoBackMessage>(this, x => this.GoToPreviousViewer());
            this.MessengerInstance.Register<RefreshCurrentViewMessage>(this, x => this.RefreshCurrent());
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
                new PaneMainItem("All", PackIconBoxIconsKind.SolidBook, "All Books", null),
                new PaneMainItem("Favorite", PackIconFontAwesomeKind.StarSolid, "Favorite Books", new Filter { Favorite = true })
            };
            this.SelectedMainPaneItem = this.MainPaneItems[0];
            this.PaneSelectionChanged();
            this.SearchOptions = ApplicationSettings.GetInstance().SearchOptions;
            filterOptions = new FilterOptions();
        }

        public ICommand AddBookCommand => new RelayCommand(this.ProcessAddBook);

        public ICommand BackCommand => new RelayCommand(this.GoToPreviousViewer);

        public ObservableCollection<ObservableUserCollection> Collections
        {
            get
            {
                using ElibContext database = ApplicationSettings.CreateContext();
                return new ObservableCollection<ObservableUserCollection>(database.UserCollections.ToList().Select(c => new ObservableUserCollection(c)).ToList());
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

        public ICommand FilterBooksCommand => new RelayCommand(this.HandleFilterButton);

        public ICommand ClearSelectedBooksCommand => new RelayCommand(this.HandleClearSelected);

        private void HandleClearSelected()
        {
            this.selector.Clear();
            this.SelectedMainPaneItem = this.MainPaneItems[0];
            MainPaneItems.Remove(selectedMainItem);
            PaneSelectionChanged();
        }

        public SearchOptions SearchOptions
        {
            get => this.searchOptions;
            set => this.Set(() => this.SearchOptions, ref this.searchOptions, value);
        }

        public ObservableUserCollection SelectedCollection
        {
            get => this.selectedCollection;

            set
            {
                this.Set(ref this.selectedCollection, value);
                if (this.selectedCollection != null)
                {
                    this.viewerHistory.Clear();

                    Filter filter = new Filter
                    {
                        CollectionIds = new List<int> { selectedCollection.Id }
                    };

                    this.CurrentViewer = new BookViewerViewModel($"Collection {this.selectedCollection.Tag}", filter, this.selector);
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
            this.SelectedCollection = this.Collections.FirstOrDefault(c => c.Id == message.CollectionId);
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

        private void ProcessSearchInput(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                token = token.ToLower();
                this.SearchOptions.Token = token;
                if(this.isInSearchResults)
                {
                    this.CurrentViewer.Search(this.SearchOptions);
                }
                else
                {
                    var viewerState = ViewerState.ToState(this.CurrentViewer);
                    this.viewerHistory.Push(viewerState);

                    this.CurrentViewer = viewerState.GetViewModel(this.selector);
                    this.CurrentViewer.Search(this.SearchOptions);
                    isInSearchResults = true;
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
            this.SearchOptions.Token = "";
            this.CurrentViewer = this.viewerHistory.Pop().GetViewModel(selector);
            this.CurrentViewer.Refresh();
        }

        private async void HandleAuthorSelection(AuthorSelectedMessage obj)
        {
            using ElibContext elibContext = ApplicationSettings.CreateContext();
            Author x = await elibContext.Authors.FindAsync(obj.AuthorId);

            string viewerCaption = $"Books by {x.Name}";
            if (viewerCaption == this.CurrentViewer.Caption)
            {
                return;
            }

            this.viewerHistory.Push(ViewerState.ToState(this.CurrentViewer));

            Filter filter = new Filter
            {
                AuthorIds = new List<int> { x.Id }
            };

            this.CurrentViewer = new BookViewerViewModel(viewerCaption, filter, this.selector);
        }

        private async void HandleSeriesSelection(SeriesSelectedMessage obj)
        {
            using ElibContext elibContext = ApplicationSettings.CreateContext();
            BookSeries x = await elibContext.Series.FindAsync(obj.SeriesId);

            string viewerCaption = $"{x.Name} Series";
            if (viewerCaption == this.CurrentViewer.Caption)
            {
                return;
            }

            Filter filter = new Filter
            {
                SeriesIds = new List<int> { x.Id }
            };

            this.viewerHistory.Push(ViewerState.ToState(this.CurrentViewer));
            this.CurrentViewer = new BookViewerViewModel(viewerCaption, filter, this.selector);
        }

        private void PaneSelectionChanged()
        {
            if (this.SelectedMainPaneItem == null)
            {
                return;
            }

            this.RaisePropertyChanged(() => this.IsSelectedBooksViewer);
            this.viewerHistory.Clear();

            Filter filter = SelectedMainPaneItem.Filter ?? new Filter();

            if (filterOptions != null && filter != null)
            {
                if (filterOptions.ShowAll) filter.Read = null;
                else if (filterOptions.ShowRead) filter.Read = true;
                else if (filterOptions.ShowUnread) filter.Read = false;

                filter.SortByAuthor = filterOptions.SortByAuthor;
                filter.SortByImportOrder = filterOptions.SortByImportTime;
                filter.SortBySeries = filterOptions.SortBySeries;
                filter.SortByTitle = filterOptions.SortByTitle;
                filter.Ascending = filterOptions.Ascending;
            }

            this.CurrentViewer = new BookViewerViewModel(
                this.SelectedMainPaneItem.ViewerCaption,
                filter,
                this.selector);
        }

        private readonly SemaphoreSlim slim = new SemaphoreSlim(1, 1);

        private async void RefreshCurrent()
        {
            if (slim.CurrentCount == 0)
                return;

            await slim.WaitAsync();
            if (this.SelectedCollection == null && this.SelectedMainPaneItem == null)
            {
                this.SelectedMainPaneItem = this.MainPaneItems[0];
            }

            if (this.CurrentViewer != null)
            {
                Filter newFilter = this.CurrentViewer.Filter ?? new Filter();

                if (filterOptions != null)
                {
                    if (filterOptions.ShowAll) newFilter.Read = null;
                    else if (filterOptions.ShowRead) newFilter.Read = true;
                    else if (filterOptions.ShowUnread) newFilter.Read = false;

                    newFilter.SortByAuthor = filterOptions.SortByAuthor;
                    newFilter.SortByImportOrder = filterOptions.SortByImportTime;
                    newFilter.SortBySeries = filterOptions.SortBySeries;
                    newFilter.SortByTitle = filterOptions.SortByTitle;
                    newFilter.Ascending = filterOptions.Ascending;
                }

                this.CurrentViewer.Refresh();
            }
            slim.Release();
        }

        private async void HandleFilterButton()
        {
            FilterOptionsDialog dialog = new FilterOptionsDialog();
            dialog.DataContext = new FilterOptionsDialogViewModel(this.filterOptions, f => this.filterOptions = f);
            await DialogCoordinator.Instance.ShowMetroDialogAsync(Application.Current.MainWindow.DataContext, dialog);
        }
    }
}