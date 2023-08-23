using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Valyreon.Elib.DataLayer;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Mvvm.Messaging;
using Valyreon.Elib.Wpf.BindingItems;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ViewModels.Dialogs;
using Valyreon.Elib.Wpf.Views.Dialogs;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class BookViewerViewModel : ViewModelBase, IViewer
    {
        private readonly Selector selector;
        private string caption;
        private Book lastSelectedBook;
        private double scrollVerticalOffset;
        private bool dontLoad = false;
        private static FilterOptions filterOptions = new FilterOptions();
        private bool isResultEmpty = false;

        private Action backAction;

        public Action Back
        {
            get => backAction;
            set
            {
                Set(() => Back, ref backAction, value);
                RaisePropertyChanged(() => IsBackEnabled);
            }
        }

        public BookViewerViewModel(FilterParameters filter, Selector selector)
        {
            Filter = filter;
            SearchText = filter.SearchText;
            Books = new ObservableCollection<Book>();
            this.selector = selector;
            ApplyFilterOptionsToFilter(filterOptions, filter);
            MessengerInstance.Register<RefreshCurrentViewMessage>(this, _ =>
            {
                if (!isLoading)
                {
                    Refresh();
                }
            });
        }

        public ICommand BackCommand => new RelayCommand(Back);

        public ObservableCollection<Book> Books { get; set; }

        public ICommand GoToAuthor =>
            new RelayCommand<ICollection<Author>>(a => Messenger.Default.Send(new AuthorSelectedMessage(a.First())));

        public ICommand GoToSeries =>
            new RelayCommand<BookSeries>(s => Messenger.Default.Send(new SeriesSelectedMessage(s)));

        public ICommand LoadMoreCommand => new RelayCommand(LoadMore);

        public ICommand OpenBookDetails => new RelayCommand<Book>(HandleBookClick);

        public bool IsBackEnabled => Back != null;

        public ICommand RefreshCommand => new RelayCommand(Refresh);

        public ICommand FilterBooksCommand => new RelayCommand(HandleFilter);

        public ICommand ClearSelectedBooksCommand => new RelayCommand(HandleClearButton);

        private async void HandleDeleteButton()
        {
            var dialog = new DeleteBooksDialog();
            IList<Book> selectedBooks = null;
            using (var uow = await App.UnitOfWorkFactory.CreateAsync())
            {
                selectedBooks = await selector.GetSelectedBooks(uow);
            }

            var deleteDialogViewModel = new DeleteBooksDialogViewModel(selectedBooks, dialog);
            deleteDialogViewModel.SetActionOnClose(() =>
            {
                HandleClearButton();
                Messenger.Default.Send(new RefreshSidePaneCollectionsMessage());
            });
            dialog.DataContext = deleteDialogViewModel;
            await DialogCoordinator.Instance.ShowMetroDialogAsync(System.Windows.Application.Current.MainWindow.DataContext, dialog);
        }

        public bool IsSelectedBooksViewer => Filter.Selected == true;

        public bool IsResultEmpty
        {
            get => isResultEmpty;
            set => Set(() => IsResultEmpty, ref isResultEmpty, value);
        }

        private void HandleClearButton()
        {
            selector.Clear();
            Messenger.Default.Send(new ResetPaneSelectionMessage());
            Messenger.Default.Send(new BookSelectedMessage());
        }

        public double ScrollVertical
        {
            get => scrollVerticalOffset;
            set => Set(() => ScrollVertical, ref scrollVerticalOffset, value);
        }

        public ICommand SelectBookCommand => new RelayCommand<Book>(b =>
        {
            b.IsMarked = !b.IsMarked;
            HandleSelectBook(b);
        });

        private FilterParameters filter = null;

        public FilterParameters Filter
        {
            get => filter;
            set
            {
                filter = value;
                if (Books != null)
                {
                    Refresh();
                }
            }
        }

        public string Caption
        {
            get => caption;
            set => Set(() => Caption, ref caption, value);
        }

        private string subCaption;

        public string SubCaption
        {
            get => subCaption;
            set => Set(() => SubCaption, ref subCaption, value);
        }

        private string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                Set(() => SearchText, ref searchText, value);
                Filter = filter with { SearchText = value };
            }
        }

        public int CurrentCount => Books.Count;

        private void HandleSelectBook(Book obj)
        {
            var isSelected = selector.Select(obj);
            var isThisSelectedView = Filter.Selected == true;
            if (isThisSelectedView && !isSelected && Books.Count == 1)
            {
                MessengerInstance.Send(new ResetPaneSelectionMessage());
            }
            else if (isThisSelectedView && !isSelected && Books.Count > 1)
            {
                Books.Remove(obj);
                UpdateSubcaption();
            }
            else
            {
                lastSelectedBook = obj;
            }

            MessengerInstance.Send(new BookSelectedMessage());
        }

        private void HandleBookClick(Book arg)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                arg.IsMarked = !arg.IsMarked;
                HandleSelectBook(arg);
                RaisePropertyChanged(() => arg.IsMarked);
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var lastSelectedIndex = lastSelectedBook == null ? 0 : Books.IndexOf(lastSelectedBook);
                var currentIndex = Books.IndexOf(arg);
                var ascIndexArray = new int[2];
                ascIndexArray[0] = currentIndex > lastSelectedIndex ? lastSelectedIndex : currentIndex;
                ascIndexArray[1] = currentIndex > lastSelectedIndex ? currentIndex : lastSelectedIndex;

                for (var i = ascIndexArray[0]; i <= ascIndexArray[1]; i++)
                {
                    var book = Books.ElementAt(i);
                    book.IsMarked = true;
                    HandleSelectBook(book);
                    RaisePropertyChanged(() => book.IsMarked);
                }
            }
            else
            {
                Messenger.Default.Send(new ShowBookDetailsMessage(arg));
            }
        }

        private async void LoadMore()
        {
            if (dontLoad)
            {
                return;
            }

            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            IEnumerable<Book> results;
            if (filter.Selected.HasValue && filter.Selected == true)
            {
                dontLoad = true;
                results = await uow.BookRepository.FindAsync(selector.SelectedIds);
            }
            else
            {
                results = await uow.BookRepository.FindPageByFilterAsync(filter, Books.Count, 25);
            }

            if (!results.Any())
            {
                IsResultEmpty = Books.Count == 0;
                return;
            }

            foreach (var book in results)
            {
                book.Authors = new ObservableCollection<Author>(await uow.AuthorRepository.GetAuthorsOfBookAsync(book.Id));
                if (book.SeriesId.HasValue)
                {
                    book.Series = await uow.SeriesRepository.FindAsync(book.SeriesId.Value);
                }

                if (book.CoverId.HasValue)
                {
                    book.Cover = await uow.CoverRepository.FindAsync(book.CoverId.Value);
                }

                selector.SetMarked(book);
                Books.Add(book);
                await Task.Delay(10);
            }

            IsResultEmpty = Books.Count == 0;
            isLoading = false;
        }
        private volatile bool isLoading;

        public async void Refresh()
        {
            if(isLoading)
            {
                return;
            }
            isLoading = true;
            ScrollVertical = 0;
            Books.Clear();

            using(var uow = await App.UnitOfWorkFactory.CreateAsync())
            {
                uow.ClearCache();
            }

            UpdateSubcaption();
            LoadMore();
        }

        private async void HandleFilter()
        {
            var dialog = new FilterOptionsDialog
            {
                DataContext = new FilterOptionsDialogViewModel(filterOptions, f =>
                {
                    filterOptions = f;
                    ApplyFilterOptionsToFilter(f, filter);
                    Refresh();
                })
            };
            await DialogCoordinator.Instance.ShowMetroDialogAsync(System.Windows.Application.Current.MainWindow.DataContext, dialog);
        }

        public ICommand ExportSelectedBooksCommand => new RelayCommand(HandleExport);

        private async void HandleExport()
        {
            var dialog = new ExportOptionsDialog();
            using (var uow = await App.UnitOfWorkFactory.CreateAsync())
            {
                dialog.DataContext = new ExportOptionsDialogViewModel(await selector.GetSelectedBooks(uow), dialog);
            }
            await DialogCoordinator.Instance.ShowMetroDialogAsync(System.Windows.Application.Current.MainWindow.DataContext, dialog);
        }

        public ICommand AddBookCommand => new RelayCommand(ProcessAddBook);

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

        private void ApplyFilterOptionsToFilter(FilterOptions f, FilterParameters p)
        {
            if (f != null && p != null)
            {
                if (f.ShowAll)
                {
                    p.Read = null;
                }
                else if (f.ShowRead)
                {
                    p.Read = true;
                }
                else if (f.ShowUnread)
                {
                    p.Read = false;
                }

                p.SortByAuthor = f.SortByAuthor;
                p.SortByImportOrder = f.SortByImportTime;
                p.SortBySeries = f.SortBySeries;
                p.SortByTitle = f.SortByTitle;
                p.Ascending = f.Ascending;
            }
        }

        public void Clear()
        {
            Books.Clear();
            ScrollVertical = 0;
        }

        private async void UpdateSubcaption()
        {
            var count = 0;
            if (!IsSelectedBooksViewer)
            {
                using var uow = await App.UnitOfWorkFactory.CreateAsync();
                count = await uow.BookRepository.CountAsync(filter);
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

        public Func<IViewer> GetCloneFunction(Selector selector)
        {
            var filterClone = Filter with { };
            var caption = this.caption;
            return () => new BookViewerViewModel(filterClone, selector) { Caption = caption };
        }

        public void Dispose()
        {
            MessengerInstance.Unregister(this);
        }
    }
}
