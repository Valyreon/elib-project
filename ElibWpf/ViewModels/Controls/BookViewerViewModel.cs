using Domain;
using GalaSoft.MvvmLight;
using Models.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElibWpf.ViewModels.Controls
{
    public class BookViewerViewModel: ViewModelBase, ISearchable
    {
        private Func<Book, bool> defaultQuery;

        private string caption;
        public string Caption
        {
            get => caption;
            set => Set(ref caption, value);
        }

        public BookViewerViewModel(string caption, Func<Book, bool> defaultQuery)
        {
            this.caption = caption;
            this.defaultQuery = defaultQuery;
            Books = App.Database.Books.Include("UserCollections").Include("Series").Include("Authors").Where(defaultQuery).ToList();
        }

        public List<Book> Books { get; set; } = new List<Book>();

        public void Search(string token, SearchOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string CurrentNumberOfBooks
        {
            get => Books.Count.ToString();
        }

        public string NumberOfBooks
        {
            get => App.Database.Books.Count().ToString();
        }
    }
}
