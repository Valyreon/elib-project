using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DataLayer;
using Domain;
using ElibWpf.Messages;
using Models;
using Models.Options;
using MVVMLibrary;
using MVVMLibrary.Messaging;

namespace ElibWpf.ViewModels.Controls
{
    public class BookViewerViewModel : ViewModelBase, IViewer
    {
        private readonly Selector selector;
        private string caption;
        private Book lastSelectedBook;
        private string numberOfBooks;
        private double scrollVerticalOffset;
        private bool dontLoad = false;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1,1);

        public BookViewerViewModel(string caption, FilterParameters filter, Selector selector)
        {
            this.Caption = caption;
            this.Filter = filter;
            this.Books = new ObservableCollection<Book>();
            this.selector = selector;
        }

        public ObservableCollection<Book> Books { get; set; }

        public ICommand GoToAuthor =>
            new RelayCommand<ICollection<Author>>(a => Messenger.Default.Send(new AuthorSelectedMessage(a.First())));

        public ICommand GoToSeries =>
            new RelayCommand<BookSeries>(s => Messenger.Default.Send(new SeriesSelectedMessage(s)));

        public ICommand LoadMoreCommand => new RelayCommand(this.LoadMore);

        public ICommand OpenBookDetails => new RelayCommand<Book>(this.HandleBookClick);

        public double ScrollVertical
        {
            get => this.scrollVerticalOffset;
            set => this.Set(() => ScrollVertical, ref this.scrollVerticalOffset, value);
        }

        public ICommand SelectBookCommand => new RelayCommand<Book>(b =>
        {
            b.IsMarked = !b.IsMarked;
            this.HandleSelectBook(b);
        });

        private FilterParameters filter = null;
        public FilterParameters Filter
        {
            get => filter;
            set
            {
                filter = value;
                if (Books != null)
                    this.Refresh();
            }
        }

        public string Caption
        {
            get => this.caption;
            set => this.Set(() => Caption, ref this.caption, value);
        }

        public string NumberOfBooks
        {
            get => this.numberOfBooks;
            set => this.Set(() => NumberOfBooks, ref this.numberOfBooks, value);
        }
        public int CurrentCount => Books.Count;

        private void HandleSelectBook(Book obj)
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

        private void HandleBookClick(Book arg)
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
                    Book book = this.Books.ElementAt(i);
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
            if (semaphore.CurrentCount == 0)
                return;

            await semaphore.WaitAsync();
            string callingMethod = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name;
            Console.WriteLine(callingMethod);
            List<Book> bookList = null;

            if (dontLoad) return;

            await Task.Factory.StartNew(() =>
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                if (filter.Selected.HasValue && filter.Selected == true)
                {
                    bookList = uow.BookRepository.GetBooks(selector.SelectedIds).ToList();
                    dontLoad = true;
                }
                else
                    bookList = uow.BookRepository.FindPageByFilter(filter, Books.Count, 25).ToList();
            });

            using var uow = ApplicationSettings.CreateUnitOfWork();
            foreach (Book item in bookList)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.Books.Add(this.selector.SetMarked(item).LoadMembers(uow));
                });
            }

            if (this.Books.Count == 0)
            {
                this.MessengerInstance.Send(new ShowDialogMessage("No matches",
                        "No books found matching the search conditions."));
                this.MessengerInstance.Send(new GoBackMessage());
            }
            semaphore.Release();
        }

        public void Refresh()
        {
            this.ScrollVertical = 0;
            this.Books.Clear();
            this.LoadMore();
        }

        public void Clear()
        {
            this.Books.Clear();
        }

        public void Search(SearchParameters searchOptions)
        {
            this.filter.SearchParameters = searchOptions;
            this.Filter.SearchParameters.Token = searchOptions.Token;
            if (!string.IsNullOrEmpty(searchOptions.Token))
            {
                this.Caption = $"Search results for '{searchOptions.Token}'";
                this.filter.SearchParameters.SearchByAuthor = searchOptions.SearchByAuthor;
                this.filter.SearchParameters.SearchByTitle = searchOptions.SearchByTitle;
                this.filter.SearchParameters.SearchBySeries = searchOptions.SearchBySeries;
            }

            this.Refresh();
        }
    }
}