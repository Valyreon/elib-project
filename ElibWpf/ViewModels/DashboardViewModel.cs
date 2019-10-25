﻿using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private ObservableCollection<IPageViewModel> _pages = new ObservableCollection<IPageViewModel>();

        /// <summary>
        /// Pages shown into the interface
        /// </summary>
        public ObservableCollection<IPageViewModel> Pages
        {
            get => _pages;
            private set { Set(() => Pages, ref _pages, value); }
        }

        public DashboardViewModel()
        {
            var books = new BooksViewModel();
            var quotes = new QuotesViewModel();
            var settings = new SettingsViewModel();
            Pages = new ObservableCollection<IPageViewModel>
            {
                books,
                quotes,
                settings
            };
        }
    }
}
