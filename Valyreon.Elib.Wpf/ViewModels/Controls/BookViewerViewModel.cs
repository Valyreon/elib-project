using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Valyreon.Elib.DataLayer.Filters;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Mvvm.Messaging;
using Valyreon.Elib.Wpf.BindingItems;
using Valyreon.Elib.Wpf.Extensions;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ViewModels.Dialogs;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class BookViewerViewModel : ViewModelBase, IViewer
    {
        private readonly ApplicationProperties applicationProperties;
        private readonly Selector selector;
        private readonly IUnitOfWorkFactory uowFactory;
        private Action backAction;
        private ObservableCollection<BookTileViewModel> books = new();
        private string caption;
        private bool dontLoad = false;
        private bool isAscendingSortDirection;
        private volatile bool isLoading;
        private bool isResultEmpty = false;
        private double scrollVerticalOffset;
        private string searchText;

        private int sortComboBoxSelectedIndex;

        private int statusComboBoxSelectedIndex;

        private string subCaption;

        public BookViewerViewModel(BookFilter filter, Selector selector, ApplicationProperties applicationProperties, IUnitOfWorkFactory uowFactory)
        {
            Filter = filter;
            this.selector = selector;
            this.applicationProperties = applicationProperties;
            this.uowFactory = uowFactory;
            MessengerInstance.Register<RefreshCurrentViewMessage>(this, _ =>
            {
                if (!isLoading)
                {
                    Refresh();
                }
            });

            MessengerInstance.Register<BookSelectedMessage>(this, HandleBookSelection);
            MessengerInstance.Register<BooksRemovedMessage>(this, HandleRemovedMessage);

            searchText = filter.SearchText;
            statusComboBoxSelectedIndex = filter.Read switch
            {
                true => 1, // READ
                false => 2, // UNREAD
                _ => 0 // ALL
            };

            if (filter.SortByImportOrder)
            {
                sortComboBoxSelectedIndex = 0;
            }
            else if (filter.SortByTitle)
            {
                sortComboBoxSelectedIndex = 1;
            }
            else if (filter.SortByAuthor)
            {
                sortComboBoxSelectedIndex = 2;
            }
            else if (filter.SortBySeries)
            {
                sortComboBoxSelectedIndex = 3;
            }

            IsAscendingSortDirection = filter.Ascending;
        }

        public ICommand AddBookCommand => new RelayCommand(ProcessAddBook);

        public Action Back
        {
            get => backAction;
            set
            {
                Set(() => Back, ref backAction, value);
                RaisePropertyChanged(() => IsBackEnabled);
            }
        }

        public ICommand BackCommand => new RelayCommand(Back);

        public ObservableCollection<BookTileViewModel> Books { get => books; set => Set(() => Books, ref books, value); }

        public string Caption
        {
            get => caption;
            set => Set(() => Caption, ref caption, value);
        }

        public ICommand ClearSelectedBooksCommand => new RelayCommand(HandleClearButton);

        public ICommand ClearSelectionCommand => new RelayCommand(HandleClearSelectedBooksInView);

        public int CurrentCount => Books.Count;

        public ICommand ExportSelectedBooksCommand => new RelayCommand(HandleExport);

        public BookFilter Filter { get; set; } = null;

        public bool IsAscendingSortDirection
        {
            get => isAscendingSortDirection;
            set
            {
                Set(() => IsAscendingSortDirection, ref isAscendingSortDirection, value);
                Filter = Filter with { Ascending = value };
            }
        }

        public bool IsBackEnabled => Back != null;

        public bool IsResultEmpty
        {
            get => isResultEmpty;
            set => Set(() => IsResultEmpty, ref isResultEmpty, value);
        }

        public bool IsSelectedBooksViewer => Filter.Selected == true;

        public ICommand LoadMoreCommand => new RelayCommand(LoadMore);

        public ICommand RefreshCommand => new RelayCommand(Refresh);

        public double ScrollVertical
        {
            get => scrollVerticalOffset;
            set => Set(() => ScrollVertical, ref scrollVerticalOffset, value);
        }

        public string SearchText
        {
            get => searchText;
            set
            {
                Set(() => SearchText, ref searchText, value);
                Filter = Filter with { SearchText = value };
                Refresh();
            }
        }

        public ICommand SelectAllCommand => new RelayCommand(HandleSelectAllBooksInView);

        public IEnumerable<FilterComboBoxOption<BookFilter>> SortComboBoxOptions { get; } = FilterComboBoxOption<BookFilter>.BookSortFilterOptions;

        public int SortComboBoxSelectedIndex
        {
            get => sortComboBoxSelectedIndex;
            set
            {
                Set(() => SortComboBoxSelectedIndex, ref sortComboBoxSelectedIndex, value);
                Filter = SortComboBoxOptions.ElementAt(value).TransformFilter(Filter);
            }
        }

        public ICommand SortDirectionChangedCommand => new RelayCommand<bool>(HandleSortDirectionChange);

        public IEnumerable<FilterComboBoxOption<BookFilter>> StatusComboBoxOptions { get; } = FilterComboBoxOption<BookFilter>.BookStatusFilterOptions;

        public int StatusComboBoxSelectedIndex
        {
            get => statusComboBoxSelectedIndex;
            set
            {
                Set(() => StatusComboBoxSelectedIndex, ref statusComboBoxSelectedIndex, value);
                Filter = StatusComboBoxOptions.ElementAt(value).TransformFilter(Filter);
            }
        }

        public string SubCaption
        {
            get => subCaption;
            set => Set(() => SubCaption, ref subCaption, value);
        }

        public void Clear()
        {
            Books.Clear();
            ScrollVertical = 0;
        }

        public void Dispose()
        {
            MessengerInstance.Unregister(this);
        }

        public Func<IViewer> GetCloneFunction()
        {
            var filterClone = Filter with { };
            var caption = this.caption;
            var selector = this.selector;
            var props = applicationProperties;
            var fact = uowFactory;
            return () => new BookViewerViewModel(filterClone, selector, props, fact) { Caption = caption };
        }

        public IFilterParameters GetFilter()
        {
            return Filter;
        }

        public void Refresh()
        {
            if (isLoading)
            {
                return;
            }
            isLoading = true;
            ScrollVertical = 0;
            Books = new();

            UpdateSubcaption();
            LoadMore();
        }

        private void HandleBookSelection(BookSelectedMessage message)
        {
            if (!message.IsShiftDown)
            {
                return;
            }

            var lastSelectedId = selector.LastSelectedId;
            var lastSelectedIndex = selector.LastSelectedId == 0 ? 0 : Books.IndexOf(Books.Single(b => b.Book.Id == lastSelectedId));
            var currentIndex = Books.Select(b => b.Book).ToList().IndexOf(message.Book);

            var ascIndexArray = new int[2];
            ascIndexArray[0] = currentIndex > lastSelectedIndex ? lastSelectedIndex : currentIndex;
            ascIndexArray[1] = currentIndex > lastSelectedIndex ? currentIndex : lastSelectedIndex;

            for (var i = ascIndexArray[0]; i <= ascIndexArray[1]; i++)
            {
                var book = Books.ElementAt(i).Book;
                if (!book.IsMarked)
                {
                    selector.Select(book);
                }
            }

            selector.LastSelectedId = lastSelectedId;
        }

        private void HandleClearButton()
        {
            selector.Clear();
            Messenger.Default.Send(new ResetPaneSelectionMessage());
            Messenger.Default.Send(new BookSelectedMessage(null));
        }

        private async void HandleClearSelectedBooksInView()
        {
            using var uow = await uowFactory.CreateAsync();
            var results = await uow.BookRepository.GetIdsByFilterAsync(Filter);

            selector.DeselectIds(results);
            foreach (var item in Books)
            {
                selector.SetMarked(item.Book);
            }
        }

        private async void HandleExport()
        {
            using var uow = await uowFactory.CreateAsync();
            var viewModel = new ExportOptionsDialogViewModel(await selector.GetSelectedBooks(uow), uowFactory);
            MessengerInstance.Send(new ShowDialogMessage(viewModel));
        }

        private void HandleRemovedMessage(BooksRemovedMessage message)
        {
            if (message.Books == null || !message.Books.Any())
            {
                return;
            }

            var toRemove = message.Books.Select(b => Books.Single(bv => bv.Book == b));
            foreach (var book in toRemove)
            {
                Books.Remove(book);
            }
        }

        private async void HandleSelectAllBooksInView()
        {
            using var uow = await uowFactory.CreateAsync();
            var results = await uow.BookRepository.GetIdsByFilterAsync(Filter);

            selector.SelectIds(results);
            foreach (var item in Books)
            {
                selector.SetMarked(item.Book);
            }
        }

        private void HandleSortDirectionChange(bool isAscending)
        {
            Filter = Filter with { Ascending = isAscending };
        }

        private async void LoadMore()
        {
            if (dontLoad)
            {
                return;
            }

            using var uow = await uowFactory.CreateAsync();
            IEnumerable<Book> results;
            if (Filter.Selected.HasValue && Filter.Selected == true)
            {
                dontLoad = true;
                results = await uow.BookRepository.FindAsync(selector.SelectedIds);
            }
            else
            {
                results = await uow.BookRepository.GetByFilterAsync(Filter, Books.Count, 25);
            }

            if (!results.Any())
            {
                IsResultEmpty = Books.Count == 0;
                isLoading = false;
                return;
            }

            foreach (var book in results)
            {
                Books.Add(new BookTileViewModel(book, selector, applicationProperties, uowFactory));
                await book.LoadBookAsync(uow);
                selector.SetMarked(book);
                await Task.Delay(5);
            }

            IsResultEmpty = Books.Count == 0;
            isLoading = false;
        }

        private void ProcessAddBook()
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Epub files|*.epub|Mobi files|*.mobi|All files|*.*",
                CheckFileExists = true,
                CheckPathExists = true,
                FilterIndex = 3,
                Multiselect = true
            };
            var result = dlg.ShowDialog();
            if (result == DialogResult.OK && dlg.FileNames.Length > 0)
            {
                MessengerInstance.Send(new OpenAddBooksFormMessage(dlg.FileNames));
            }
        }

        private async void UpdateSubcaption()
        {
            var count = 0;
            if (!IsSelectedBooksViewer)
            {
                using var uow = await uowFactory.CreateAsync();
                count = await uow.BookRepository.CountAsync(Filter);
            }
            else
            {
                count = selector.Count;
            }

            SubCaption = $"{count} book";
            if (count != 1)
            {
                SubCaption += "s";
            }

            if (Filter.Read != null)
            {
                SubCaption += ", filter is applied";
            }
        }
    }
}
