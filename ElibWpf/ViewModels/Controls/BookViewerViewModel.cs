using Domain;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Models.Options;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ElibWpf.Paging;
using System.Linq;
using System.Threading;
using GalaSoft.MvvmLight.Messaging;
using ElibWpf.Messages;
using System.Collections;
using System.Collections.Generic;

namespace ElibWpf.ViewModels.Controls
{
    public class BookViewerViewModel : ViewModelBase, IViewer
    {
        public readonly Func<Book, bool> DefaultCondition;
        private string caption;
        private int nextPage = 1;
        private string numberOfBooks;
        private double scrollVerticalOffset;

        public BookViewerViewModel(string caption, Func<Book, bool> defaultQuery)
        {
            Caption = caption;
            this.DefaultCondition = defaultQuery;
            Books = new ObservableCollection<Book>();
        }

        public ObservableCollection<Book> Books { get; set; }

        public string Caption
        {
            get => caption;
            set => Set(ref caption, value);
        }

        public ICommand LoadMoreCommand { get => new RelayCommand(this.LoadMore); }

        public string NumberOfBooks
        {
            get => numberOfBooks;
            set => Set(ref numberOfBooks, value);
        }

        public double ScrollVertical
        {
            get => scrollVerticalOffset;
            set => Set(ref scrollVerticalOffset, value);
        }
        public object Clone()
        {
            return new BookViewerViewModel(this.Caption, this.DefaultCondition);
        }

        private async void LoadMore()
        {
            var bookList = await Task.Run(() => App.Database.Books.Include("Authors").Include("Series").Include("UserCollections").Where(DefaultCondition).AsQueryable().ToPagedList(nextPage++, 30));
            NumberOfBooks = bookList.TotalCount.ToString();
            if(Books.Count < bookList.TotalCount)
            {
                foreach (var item in bookList)
                {
                    App.Current.Dispatcher.Invoke(() => Books.Add(item));
                };
            }
            ;
        }

        public ICommand GoToAuthor { get => new RelayCommand<ICollection<Author>>((ICollection<Author> a) => Messenger.Default.Send(new AuthorSelectedMessage(a.ElementAt(0)))); }

        public ICommand GoToSeries { get => new RelayCommand<BookSeries>((BookSeries a) => Messenger.Default.Send(new SeriesSelectedMessage(a))); }

        public ICommand OpenBookDetails { get => new RelayCommand<Book>((Book b) => Messenger.Default.Send(new ShowBookDetailsMessage(b))); }

    }
}
