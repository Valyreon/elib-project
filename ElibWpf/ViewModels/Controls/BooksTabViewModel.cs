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
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using Models;
using Models.Observables;
using MVVMLibrary;
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

        private SearchParameters searchOptions;
        private UserCollection selectedCollection;
        private PaneMainItem selectedMainPaneItem;
        private FilterOptions filterOptions;

        public BooksTabViewModel()
        {
            this.selector = new Selector();
            this.selectedMainItem = new PaneMainItem("Selected", PackIconFontAwesomeKind.CheckDoubleSolid, "Selected Books", new FilterParameters { Selected = true });

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
                new PaneMainItem("Favorite", PackIconFontAwesomeKind.StarSolid, "Favorite Books", new FilterParameters { Favorite = true })
            };
            this.SelectedMainPaneItem = this.MainPaneItems[0];
            this.PaneSelectionChanged();
            this.SearchOptions = new SearchParameters();
            filterOptions = new FilterOptions();
        }

        public ICommand AddBookCommand => new RelayCommand(this.ProcessAddBook);

        public ICommand BackCommand => new RelayCommand(this.GoToPreviousViewer);

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
            isSelectedMainAdded = false;
            PaneSelectionChanged();
        }

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
                    this.viewerHistory.Clear();

                    FilterParameters filter = new FilterParameters
                    {
                        CollectionId = selectedCollection.Id
                    };

                    this.CurrentViewer = new BookViewerViewModel($"Collection {this.selectedCollection.Tag}", filter, this.selector);
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

        private async void HandleExport()
        {
            ExportOptionsDialog dialog = new ExportOptionsDialog();
            using var uow = ApplicationSettings.CreateUnitOfWork();
            dialog.DataContext = new ExportOptionsDialogViewModel(this.selector.GetSelectedBooks(uow), dialog);
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
                            using var uow = ApplicationSettings.CreateUnitOfWork();
                            Book book = pBook.ToBook(uow);
                            booksToAdd.Add(book);
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Log("BOOK_PARSE_ERROR", $"\nMESSAGE:{e.Message}\nSTACK:{e.StackTrace}");
                        var content = File.ReadAllBytes(dlg.FileNames[i]);
                        booksToAdd.Add(new Book
                        {
                            Collections = new ObservableCollection<UserCollection>(),
                            File = new EFile
                            {
                                Format = Path.GetExtension(dlg.FileNames[i]),
                                Signature = Signer.ComputeHash(content),
                                RawFile = new RawFile { RawContent = content }
                            },
                            Authors = new ObservableCollection<Author>()
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

                if (isInSearchResults)
                {
                    var save = viewerHistory.Peek();
                    var viewModel = save.GetViewModel(this.selector);

                    viewModel.Search(this.SearchOptions);

                    if (viewModel.CurrentCount > 0)
                    {
                        CurrentViewer = viewModel;
                        isInSearchResults = true;
                    }
                }
                else
                {
                    var save = ViewerState.ToState(this.CurrentViewer);
                    var viewModel = save.GetViewModel(this.selector);

                    viewModel.Search(this.SearchOptions);

                    if(viewModel.CurrentCount > 0)
                    {
                        viewerHistory.Push(save);
                        CurrentViewer = viewModel;
                        isInSearchResults = true;
                    }
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
            this.CurrentViewer = this.viewerHistory.Pop().GetViewModel(selector);
        }

        private void HandleAuthorSelection(AuthorSelectedMessage obj)
        {
            using var uow = ApplicationSettings.CreateUnitOfWork();

            string viewerCaption = $"Books by {obj.Author.Name}";
            if (viewerCaption == this.CurrentViewer.Caption)
            {
                return;
            }

            this.viewerHistory.Push(ViewerState.ToState(this.CurrentViewer));

            FilterParameters filter = new FilterParameters
            {
                AuthorId = obj.Author.Id
            };

            this.CurrentViewer = new BookViewerViewModel(viewerCaption, filter, this.selector);
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

            FilterParameters filter = SelectedMainPaneItem.Filter?.Clone() ?? new FilterParameters();

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
                FilterParameters newFilter = this.CurrentViewer.Filter ?? new FilterParameters();

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