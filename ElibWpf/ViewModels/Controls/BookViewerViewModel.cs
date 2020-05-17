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
using ElibWpf.Paging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Models;
using Models.Observables;

namespace ElibWpf.ViewModels.Controls
{
    public class BookViewerViewModel : ViewModelBase, IViewer
    {
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly bool isSelectedBookView;

        private readonly Selector selector;
        private string caption;

        private ObservableBook lastSelectedBook;
        private int nextPage = 1;
        private string numberOfBooks;
        private double scrollVerticalOffset;

        public BookViewerViewModel(string caption, Func<Book, bool> defaultQuery, Selector selector,
            bool isSelectedView = false)
        {
            this.Caption = caption;
            this.DefaultCondition = defaultQuery;
            this.Books = new ObservableCollection<ObservableBook>();
            this.isSelectedBookView = isSelectedView;
            this.selector = selector;
        }

        public ObservableCollection<ObservableBook> Books { get; set; }

        public ICommand GoToAuthor =>
            new RelayCommand<ICollection<ObservableAuthor>>(a => Messenger.Default.Send(new AuthorSelectedMessage(a)));

        public ICommand GoToSeries =>
            new RelayCommand<ObservableSeries>(a => Messenger.Default.Send(new SeriesSelectedMessage(a)));

        public ICommand LoadMoreCommand => new RelayCommand(this.LoadMore);

        public ICommand OpenBookDetails => new RelayCommand<ObservableBook>(this.HandleBookClick);

        public double ScrollVertical
        {
            get => this.scrollVerticalOffset;
            set => this.Set(ref this.scrollVerticalOffset, value);
        }

        public ICommand SelectBookCommand => new RelayCommand<ObservableBook>(this.HandleSelectBook);

        public Func<Book, bool> DefaultCondition { get; }

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

        public object Clone()
        {
            return new BookViewerViewModel(this.Caption, this.DefaultCondition, this.selector);
        }

        private void HandleSelectBook(ObservableBook obj)
        {
            bool isSelected = this.selector.Select(obj);
            if (this.isSelectedBookView && !isSelected && this.Books.Count == 1)
            {
                this.MessengerInstance.Send(new ResetPaneSelectionMessage());
            }
            else if (this.isSelectedBookView && !isSelected && this.Books.Count > 1)
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
            PagedList<ObservableBook> bookList;
            await semaphoreSlim.WaitAsync();
            try
            {
                using ElibContext context = ApplicationSettings.CreateContext();
                bookList = await Task.Run(() =>
                    context.Books.Include("Authors").Include("Series").Include("UserCollections")
                        .Where(this.DefaultCondition).Select(b => this.selector.SetMarked(new ObservableBook(b))).AsQueryable()
                        .ToPagedList(this.nextPage++, 30));
            }
            finally
            {
                semaphoreSlim.Release();
            }

            this.NumberOfBooks = bookList.TotalCount.ToString();
            if (this.Books.Count < bookList.TotalCount)
            {
                foreach (ObservableBook item in bookList)
                {
                    Application.Current.Dispatcher.Invoke(() => this.Books.Add(item));
                }
            }
        }
    }
}