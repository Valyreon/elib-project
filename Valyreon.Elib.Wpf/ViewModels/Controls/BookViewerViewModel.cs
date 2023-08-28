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
using Valyreon.Elib.Wpf.Extensions;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ViewModels.Dialogs;
using Valyreon.Elib.Wpf.Views.Dialogs;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class BookViewerViewModel : ViewModelBase, IViewer
    {
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

        public BookViewerViewModel(FilterParameters filter)
        {
            Selector.Instance.LastSelectedId = 0;
            Filter = filter;
            SearchText = filter.SearchText;
            Books = new ObservableCollection<Book>();
            ApplyFilterOptionsToFilter(filterOptions, filter);
            MessengerInstance.Register<RefreshCurrentViewMessage>(this, _ =>
            {
                if (!isLoading)
                {
                    Refresh();
                }
            });

            MessengerInstance.Register<BookSelectedMessage>(this, HandleBookSelection);
            MessengerInstance.Register<BookRemovedMessage>(this, m => Books.Remove(m.Book));
        }

        public ICommand BackCommand => new RelayCommand(Back);

        public ObservableCollection<Book> Books { get; set; }

        public ICommand LoadMoreCommand => new RelayCommand(LoadMore);

        public bool IsBackEnabled => Back != null;

        public ICommand RefreshCommand => new RelayCommand(Refresh);

        public ICommand FilterBooksCommand => new RelayCommand(HandleFilter);

        public ICommand ClearSelectedBooksCommand => new RelayCommand(HandleClearButton);

        public bool IsSelectedBooksViewer => Filter.Selected == true;

        public bool IsResultEmpty
        {
            get => isResultEmpty;
            set => Set(() => IsResultEmpty, ref isResultEmpty, value);
        }

        private void HandleClearButton()
        {
            Selector.Instance.Clear();
            Messenger.Default.Send(new ResetPaneSelectionMessage());
            Messenger.Default.Send(new BookSelectedMessage(null));
        }

        public double ScrollVertical
        {
            get => scrollVerticalOffset;
            set => Set(() => ScrollVertical, ref scrollVerticalOffset, value);
        }

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

        private void HandleBookSelection(BookSelectedMessage message)
        {
            if (!message.IsShiftDown)
            {
                return;
            }

            var lastSelectedId = Selector.Instance.LastSelectedId;
            var lastSelectedIndex = Selector.Instance.LastSelectedId == 0 ? 0 : Books.IndexOf(Books.Single(b => b.Id == lastSelectedId));
            var currentIndex = Books.IndexOf(message.Book);

            var ascIndexArray = new int[2];
            ascIndexArray[0] = currentIndex > lastSelectedIndex ? lastSelectedIndex : currentIndex;
            ascIndexArray[1] = currentIndex > lastSelectedIndex ? currentIndex : lastSelectedIndex;

            Enumerable.Range(ascIndexArray[0], ascIndexArray[1])
                .Select(i => Books.ElementAt(i))
                .Where(b => !b.IsMarked)
                .ToList()
                .ForEach(b => Selector.Instance.Select(b));

            Selector.Instance.LastSelectedId = lastSelectedId;
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
                results = await uow.BookRepository.FindAsync(Selector.Instance.SelectedIds);
            }
            else
            {
                results = await uow.BookRepository.FindPageByFilterAsync(filter, Books.Count, 25);
            }

            if (!results.Any())
            {
                IsResultEmpty = Books.Count == 0;
                isLoading = false;
                return;
            }

            foreach (var book in results)
            {
                await book.LoadBookAsync(uow);
                Selector.Instance.SetMarked(book);
                Books.Add(book);
                await Task.Delay(10);
            }

            IsResultEmpty = Books.Count == 0;
            isLoading = false;
        }

        private volatile bool isLoading;

        public async void Refresh()
        {
            if (isLoading)
            {
                return;
            }
            isLoading = true;
            ScrollVertical = 0;
            Books.Clear();

            using (var uow = await App.UnitOfWorkFactory.CreateAsync())
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
                dialog.DataContext = new ExportOptionsDialogViewModel(await Selector.Instance.GetSelectedBooks(uow), dialog);
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
                count = Selector.Instance.Count;
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

        public Func<IViewer> GetCloneFunction()
        {
            var filterClone = Filter with { };
            var caption = this.caption;
            return () => new BookViewerViewModel(filterClone) { Caption = caption };
        }

        public void Dispose()
        {
            MessengerInstance.Unregister(this);
        }
    }
}
