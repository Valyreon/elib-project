using DataLayer;
using Domain;
using GalaSoft.MvvmLight;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.ViewModels.Controls
{
    public class BooksViewModel : ViewModelBase, IPageViewModel
    {
        private string _caption = "Books";

        public string Caption
        {
            get => _caption;
            set => Set(ref _caption, value);
        }

        public List<UserCollection> Collections { get; set; }// = new ElibContext(ApplicationSettings.GetInstance().DatabasePath).UserCollections.ToList();

        private bool _isPaneOpen = true;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set => Set(ref _isPaneOpen, true);
        }
    }
}
