﻿using Domain;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Models.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ElibWpf.ViewModels.Controls
{
    public class BookViewerViewModel : ViewModelBase, ISearchable
    {
        private readonly Func<Book, bool> defaultQuery;

        private string caption;
        public string Caption
        {
            get => caption;
            set => Set(ref caption, value);
        }

        public BookViewerViewModel(string caption, Func<Book, bool> defaultQuery)
        {
            Caption = caption;
            this.defaultQuery = defaultQuery;
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

        public ICommand OnViewerLoadedCommand { get => new RelayCommand(this.Refresh); }

        public async void Refresh()
        {
            App.Current.Dispatcher.Invoke(() => Books.Clear());
            var books = await Task.Run(() => App.Database.Books.Include("UserCollections").Include("Series").Include("Authors").Where(defaultQuery));

            foreach (var x in books)
            {
                App.Current.Dispatcher.Invoke(() => Books.Add(x));
                await Task.Run(() => Thread.Sleep(15));
            }
        }
    }
}
