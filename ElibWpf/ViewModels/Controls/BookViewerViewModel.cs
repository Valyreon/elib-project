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
    public class BookViewerViewModel : ViewModelBase, ISearchable
    {
        public readonly Func<Book, bool> DefaultCondition;
        private int nextPage = 1;

        private string caption;
        public string Caption
        {
            get => caption;
            set => Set(ref caption, value);
        }

        public BookViewerViewModel(string caption, Func<Book, bool> defaultQuery)
        {
            Caption = caption;
            this.DefaultCondition = defaultQuery;
            Books = new ObservableCollection<Book>();
            Books.CollectionChanged += (a, b) => base.RaisePropertyChanged("CurrentNumberOfBooks");
        }

        public ObservableCollection<Book> Books { get; set; }

        public void Search(string token, SearchOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string CurrentNumberOfBooks
        {
            get => Books.Count.ToString();
        }

        private void Refresh()
        {
            nextPage = 1;
            Books.Clear();
            LoadMore();
        }

        public ICommand RefreshCommand { get => new RelayCommand(this.Refresh); }
        public ICommand LoadMoreCommand { get => new RelayCommand(this.LoadMore); }

        private async void LoadMore()
        {
            var bookList = await Task.Run(() => App.Database.Books.Include("Authors").Include("Series").Include("UserCollections").Where(DefaultCondition).AsQueryable().ToPagedList(nextPage++, 30));
            if (bookList[0].Authors.First().Books != null)
                throw new NullReferenceException();
            if(Books.Count < bookList.TotalCount)
            {
                foreach (var item in bookList)
                {
                    App.Current.Dispatcher.Invoke(() => Books.Add(item));
                };
            }
        }
    }
}
