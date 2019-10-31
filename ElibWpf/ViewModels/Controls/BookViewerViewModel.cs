using Domain;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Models.Options;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Controls
{
    public class BookViewerViewModel : ViewModelBase, ISearchable
    {
        public readonly Func<Book, bool> DefaultCondition;

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

    }
}
