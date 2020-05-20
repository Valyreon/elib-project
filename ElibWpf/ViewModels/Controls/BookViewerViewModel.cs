using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Migrations.History;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DataLayer;
using Domain;
using ElibWpf.Messages;
using ElibWpf.Paging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Models;
using Models.Observables;
using Models.Options;

namespace ElibWpf.ViewModels.Controls
{
    public class BookViewerViewModel : ViewModelBase, IViewer
    {
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly Selector selector;
        private string caption;
        private SearchOptions searchOptions;
        private ObservableBook lastSelectedBook;
        private int nextPage = 1;
        private string numberOfBooks;
        private double scrollVerticalOffset;

        public BookViewerViewModel(string caption, Filter filter, Selector selector)
        {
            this.Caption = caption;
            this.Filter = filter;
            this.Books = new ObservableCollection<ObservableBook>();
            this.selector = selector;
        }

        public ObservableCollection<ObservableBook> Books { get; set; }

        public ICommand GoToAuthor =>
            new RelayCommand<ICollection<ObservableAuthor>>(a => Messenger.Default.Send(new AuthorSelectedMessage(a.First().Id)));

        public ICommand GoToSeries =>
            new RelayCommand<ObservableSeries>(a => Messenger.Default.Send(new SeriesSelectedMessage(a.Id)));

        public ICommand LoadMoreCommand => new RelayCommand(this.LoadMore);

        public ICommand OpenBookDetails => new RelayCommand<ObservableBook>(this.HandleBookClick);

        public double ScrollVertical
        {
            get => this.scrollVerticalOffset;
            set => this.Set(ref this.scrollVerticalOffset, value);
        }

        public ICommand SelectBookCommand => new RelayCommand<ObservableBook>(b => {
            b.IsMarked = !b.IsMarked;
            this.HandleSelectBook(b);
        });

        public Filter Filter { get; }

        public string Caption
        {
            get => this.caption;
            set => this.Set(ref this.caption, value);
        }

        public string NumberOfBooks
        {
            get => this.numberOfBooks;
            set => this.Set(ref this.numberOfBooks, value);
        }

        private void HandleSelectBook(ObservableBook obj)
        {
            bool isSelected = this.selector.Select(obj);
            bool isThisSelectedView = Filter.Selected.HasValue && Filter.Selected.Value;
            if (isThisSelectedView && !isSelected && this.Books.Count == 1)
            {
                this.MessengerInstance.Send(new ResetPaneSelectionMessage());
            }
            else if (isThisSelectedView && !isSelected && this.Books.Count > 1)
            {
                this.Books.Remove(obj);
            }
            else
            {
                this.lastSelectedBook = obj;
            }

            this.MessengerInstance.Send(new BookSelectedMessage());
        }

        private void HandleBookClick(ObservableBook arg)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                arg.IsMarked = !arg.IsMarked;
                this.HandleSelectBook(arg);
                this.RaisePropertyChanged(() => arg.IsMarked);
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                int lastSelectedIndex = this.lastSelectedBook == null ? 0 : this.Books.IndexOf(this.lastSelectedBook);
                int currentIndex = this.Books.IndexOf(arg);
                var ascIndexArray = new int[2];
                ascIndexArray[0] = currentIndex > lastSelectedIndex ? lastSelectedIndex : currentIndex;
                ascIndexArray[1] = currentIndex > lastSelectedIndex ? currentIndex : lastSelectedIndex;

                for (int i = ascIndexArray[0]; i <= ascIndexArray[1]; i++)
                {
                    ObservableBook book = this.Books.ElementAt(i);
                    book.IsMarked = true;
                    this.HandleSelectBook(book);
                    this.RaisePropertyChanged(() => book.IsMarked);
                }
            }
            else
            {
                Messenger.Default.Send(new ShowBookDetailsMessage(arg));
            }
        }

        private async void LoadMore()
        {
            PagedList<Book> bookList;

            using ElibContext context = ApplicationSettings.CreateContext();
            IQueryable<Book> query = CreateQueryFromFilter(context.Books.Include("Authors").Include("Series").Include("UserCollections"));

            await semaphoreSlim.WaitAsync();
            try
            {
                bookList = await Task.Run(() => query.ToPagedList(this.nextPage++, 25));
            }
            finally
            {
                semaphoreSlim.Release();
            }

            this.NumberOfBooks = bookList.TotalCount.ToString();
            if (this.Books.Count < bookList.TotalCount)
            {
                foreach (Book item in bookList)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        this.Books.Add(this.selector.SetMarked(new ObservableBook(item)));
                    });
                }
            }

            if(this.Books.Count == 0)
            {
                this.MessengerInstance.Send(new ShowDialogMessage("No matches",
                        "No books found matching the search conditions."));
                this.MessengerInstance.Send(new GoBackMessage());
            }
        }

        public void Refresh()
        {
            this.nextPage = 1;
            this.ScrollVertical = 0;
            this.Books.Clear();
            this.LoadMore();
        }

        public void Clear()
        {
            this.Books.Clear();
        }

        private IQueryable<Book> CreateQueryFromFilter(IQueryable<Book> initial)
        {
            if (Filter == null)
                return initial;

            IQueryable<Book> aggregateQuery = initial;
            if (Filter.BookIds != null)
            {
                aggregateQuery = aggregateQuery.Where(b => Filter.BookIds.Contains(b.Id));
            }

            if(Filter.AuthorIds != null)
            {
                aggregateQuery = aggregateQuery.Where(b => b.Authors.Select(a => a.Id).Any(x => Filter.AuthorIds.Contains(x)));
            }

            if(Filter.CollectionIds != null)
            {
                aggregateQuery = aggregateQuery.Where(b => b.UserCollections.Select(a => a.Id).Any(x => Filter.CollectionIds.Contains(x)));
            }

            if (Filter.SeriesIds != null)
            {
                aggregateQuery = aggregateQuery.Where(b => b.SeriesId != null && Filter.SeriesIds.Contains(b.SeriesId.Value));
            }

            if (Filter.Read != null)
            {
                aggregateQuery = aggregateQuery.Where(b => b.IsRead == Filter.Read.Value);
            }

            if (Filter.Favorite != null)
            {
                aggregateQuery = aggregateQuery.Where(b => b.IsFavorite == Filter.Favorite.Value);
            }

            if (Filter.Selected != null)
            {
                aggregateQuery = aggregateQuery.Where(b => selector.SelectedIds.Contains(b.Id));
            }

            if (Filter.SortByImportOrder)
            {
                aggregateQuery = Filter.Ascending ? aggregateQuery.OrderBy(b => b.Id) : aggregateQuery.OrderByDescending(b => b.Id);
            }
            else if(Filter.SortByTitle)
            {
                aggregateQuery = Filter.Ascending ? aggregateQuery.OrderBy(b => b.Title) : aggregateQuery.OrderByDescending(b => b.Title);
            }
            else if(Filter.SortBySeries)
            {
                aggregateQuery = Filter.Ascending ? aggregateQuery.OrderBy(b => b.Series.Name) : aggregateQuery.OrderByDescending(b => b.Series.Name);
            }
            else if(Filter.SortByAuthor)
            {
                aggregateQuery = Filter.Ascending ? aggregateQuery.OrderBy(b => b.Authors.FirstOrDefault().Name) : aggregateQuery.OrderByDescending(b => b.Authors.FirstOrDefault().Name);
            }

            if(searchOptions != null && !string.IsNullOrEmpty(searchOptions.Token))
            {
                aggregateQuery = aggregateQuery.Where(x => (this.searchOptions.SearchByName && x.Title.ToLower().Contains(this.searchOptions.Token) ||
                                                   this.searchOptions.SearchByAuthor &&
                                                   x.Authors.Any(a => a.Name.ToLower().Contains(this.searchOptions.Token)) || this.searchOptions.SearchBySeries &&
                                                   x.Series != null &&
                                                   x.Series.Name.ToLower().Contains(this.searchOptions.Token)));
            }
            return aggregateQuery;
        }

        public void Search(SearchOptions searchOptions)
        {
            this.searchOptions = searchOptions;
            if (!string.IsNullOrEmpty(searchOptions.Token))
                this.Caption = $"Search results for '{searchOptions.Token}'";
            this.Refresh();
        }
    }
}