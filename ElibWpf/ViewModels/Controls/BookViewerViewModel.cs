using DataLayer;
using Domain;

using ElibWpf.Messages;
using ElibWpf.Paging;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Models;
using Models.Observables;
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

        ObservableBook lastSelectedBook = null;

        public BookViewerViewModel(string caption, Func<Book, bool> defaultQuery, Selector selector, bool isSelectedView = false)
        {
            Caption = caption;
            this.DefaultCondition = defaultQuery;
            Books = new ObservableCollection<ObservableBook>();
            isSelectedBookView = isSelectedView;
            this.selector = selector;
        }

        public ObservableCollection<ObservableBook> Books { get; set; }

        public string Caption
        {
            get => caption;
            set => Set(ref caption, value);
        }

        public ICommand GoToAuthor { get => new RelayCommand<ICollection<ObservableAuthor>>((ICollection<ObservableAuthor> a) => Messenger.Default.Send(new AuthorSelectedMessage(a))); }
        public ICommand GoToSeries { get => new RelayCommand<ObservableSeries>((ObservableSeries a) => Messenger.Default.Send(new SeriesSelectedMessage(a))); }
        public ICommand SelectBookCommand { get => new RelayCommand<ObservableBook>(this.HandleSelectBook); }

        private void HandleSelectBook(ObservableBook obj)
        {
            obj.IsMarked = !obj.IsMarked;
            bool isSelected = selector.Select(obj);
            if (isSelectedBookView && !isSelected && Books.Count == 1)
            {
                MessengerInstance.Send(new ResetPaneSelectionMessage());
            }
            else if (isSelectedBookView && !isSelected && Books.Count > 1)
            {
                Books.Remove(obj);
            }
            else
            {
                lastSelectedBook = obj;
            }

            MessengerInstance.Send(new BookSelectedMessage());
        }

        public ICommand LoadMoreCommand { get => new RelayCommand(this.LoadMore); }

        public string NumberOfBooks
        {
            get => numberOfBooks;
            set => Set(ref numberOfBooks, value);
        }

        public ICommand OpenBookDetails { get => new RelayCommand<ObservableBook>(this.HandleBookClick); }

        private void HandleBookClick(ObservableBook arg)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                arg.IsMarked = !arg.IsMarked;
                HandleSelectBook(arg);
            }
            else if(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var lastSelectedIndex = lastSelectedBook==null ? 0 : Books.IndexOf(lastSelectedBook);
                var currentIndex = Books.IndexOf(arg);
                int[] ascIndexArray = new int[2];
                ascIndexArray[0] = currentIndex > lastSelectedIndex ? lastSelectedIndex : currentIndex;
                ascIndexArray[1] = currentIndex > lastSelectedIndex ? currentIndex : lastSelectedIndex;

                for(int i=ascIndexArray[0]; i<= ascIndexArray[1]; i++)
                {
                    var book = Books.ElementAt(i);
                    book.IsMarked = true;
                    RaisePropertyChanged(() => book.IsMarked);
                    HandleSelectBook(book);
                }
            }
            else
            {
                Messenger.Default.Send(new ShowBookDetailsMessage(arg));
            }
        }

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
            PagedList<ObservableBook> bookList;
            await semaphoreSlim.WaitAsync();
            try
            {
                using ElibContext dbcontext = ApplicationSettings.CreateContext();
                bookList = await Task.Run(() => dbcontext.Books.Include("Authors").Include("Series").Include("UserCollections").Where(DefaultCondition).Select(b => selector.SetMarked(new ObservableBook(b))).AsQueryable().ToPagedList(nextPage++, 30));
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