using DataLayer;
using Domain;

using ElibWpf.Messages;
using ElibWpf.Paging;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Controls
{
    public class BookViewerViewModel : ViewModelBase, IViewer
    {
        public Func<Book, bool> DefaultCondition { get; }
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private string caption;
        private int nextPage = 1;
        private string numberOfBooks;
        private double scrollVerticalOffset;
        private readonly bool isSelectedBookView;

        private readonly Selector selector;

        public BookViewerViewModel(string caption, Func<Book, bool> defaultQuery, Selector selector, bool isSelectedView = false)
        {
            Caption = caption;
            this.DefaultCondition = defaultQuery;
            Books = new ObservableCollection<Book>();
            isSelectedBookView = isSelectedView;
            this.selector = selector;
            MessengerInstance.Register<BookEditDoneMessage>(this, this.EditedBookRefresh);
        }

        private void EditedBookRefresh(BookEditDoneMessage obj)
        {
            //RaisePropertyChanged(() => Books);
        }

        public ObservableCollection<Book> Books { get; set; }

        public string Caption
        {
            get => caption;
            set => Set(ref caption, value);
        }

        public ICommand GoToAuthor { get => new RelayCommand<ICollection<Author>>((ICollection<Author> a) => Messenger.Default.Send(new AuthorSelectedMessage(a))); }
        public ICommand GoToSeries { get => new RelayCommand<BookSeries>((BookSeries a) => Messenger.Default.Send(new SeriesSelectedMessage(a))); }
        public ICommand SelectBookCommand { get => new RelayCommand<Book>(this.HandleSelectBook); }

        private void HandleSelectBook(Book obj)
        {
            bool isSelected = selector.Select(obj);
            if (isSelectedBookView && !isSelected && Books.Count == 1)
            {
                MessengerInstance.Send(new BookSelectedMessage());
                MessengerInstance.Send(new ResetPaneSelectionMessage());
            }
            else if (isSelectedBookView && !isSelected && Books.Count > 1)
            {
                MessengerInstance.Send(new BookSelectedMessage());
                Books.Remove(obj);
            }
            else
                MessengerInstance.Send(new BookSelectedMessage());
        }

        public ICommand LoadMoreCommand { get => new RelayCommand(this.LoadMore); }

        public string NumberOfBooks
        {
            get => numberOfBooks;
            set => Set(ref numberOfBooks, value);
        }

        public ICommand OpenBookDetails { get => new RelayCommand<Book>((Book b) => Messenger.Default.Send(new ShowBookDetailsMessage(b))); }

        public double ScrollVertical
        {
            get => scrollVerticalOffset;
            set => Set(ref scrollVerticalOffset, value);
        }

        public object Clone()
        {
            return new BookViewerViewModel(this.Caption, this.DefaultCondition, selector);
        }

        private async void LoadMore()
        {
            PagedList<Book> bookList;
            await semaphoreSlim.WaitAsync();
            try
            {
                using ElibContext database = ApplicationSettings.CreateContext();
                bookList = await Task.Run(() => database.Books.Include("Authors").Include("Series").Include("UserCollections").Where(DefaultCondition).Select(b => selector.SetMarked(b)).AsQueryable().ToPagedList(nextPage++, 30));
            }
            finally
            {
                semaphoreSlim.Release();
            }

            NumberOfBooks = bookList.TotalCount.ToString();
            if (Books.Count < bookList.TotalCount)
            {
                foreach (var item in bookList)
                {
                    App.Current.Dispatcher.Invoke(() => Books.Add(item));
                };
            }
        }
    }
}