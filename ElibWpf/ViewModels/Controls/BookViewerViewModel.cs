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

namespace ElibWpf.ViewModels.Controls
{
    public class BookViewerViewModel : ViewerViewModel
    {
        public readonly Func<Book, bool> DefaultCondition;
        private int nextPage = 1;

        private string caption;
        public string Caption
        {
            get => caption;
            set => Set(ref caption, value);
        }

        private double scrollVerticalOffset;
        public double ScrollVertical
        {
            get => scrollVerticalOffset;
            set => Set(ref scrollVerticalOffset, value);
        }

        public BookViewerViewModel(string caption, Func<Book, bool> defaultQuery)
        {
            Caption = caption;
            this.DefaultCondition = defaultQuery;
            Books = new ObservableCollection<Book>();
        }

        public ObservableCollection<Book> Books { get; set; }

        private string numberOfBooks;
        public string NumberOfBooks
        {
            get => numberOfBooks;
            set => Set(ref numberOfBooks, value);
        }

        public ICommand RefreshCommand { get => new RelayCommand(this.RaiseRefreshEvent); }

        public ICommand LoadMoreCommand { get => new RelayCommand(this.LoadMore); }

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
        }

        public override object Clone()
        {
            return new BookViewerViewModel(this.Caption, this.DefaultCondition);
        }
    }
}
