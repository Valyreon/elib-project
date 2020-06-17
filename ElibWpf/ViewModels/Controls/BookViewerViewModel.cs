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
using Models;

using Models.Options;
using MVVMLibrary;
using MVVMLibrary.Messaging;
using Filter = Models.Options.Filter;

namespace ElibWpf.ViewModels.Controls
{
    public class BookViewerViewModel : ViewModelBase, IViewer
    {
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly Selector selector;
        private string caption;
        private SearchOptions searchOptions;
        private Book lastSelectedBook;
        private int nextPage = 1;
        private string numberOfBooks;
        private double scrollVerticalOffset;

        public BookViewerViewModel(string caption, FilterAlt filter, Selector selector)
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

        private FilterAlt filter = null;
        public FilterAlt Filter
        {
            get => filter;
            set
            {
                filter = value;
                if(Books != null)
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

        private void LoadMore()
        {
            List<Book> bookList;

            using UnitOfWork uow = ApplicationSettings.CreateUnitOfWork();
            bookList = uow.BookRepository.FindPageByFilter(filter, Books.LastOrDefault()).ToList();

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

        public void Search(SearchOptions searchOptions)
        {
            this.searchOptions = searchOptions;
            if (!string.IsNullOrEmpty(searchOptions.Token))
                this.Caption = $"Search results for '{searchOptions.Token}'";
            this.Refresh();
        }
    }
}