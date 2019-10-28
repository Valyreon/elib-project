using DataLayer;
using Domain;
using GalaSoft.MvvmLight;
using Models;
using System.Collections.Generic;
using System.Linq;

namespace ElibWpf.ViewModels.Controls
{
    public class BooksTabViewModel : ViewModelBase, IPageViewModel
    {
        private string _caption = "Books";

        public string Caption
        {
            get => _caption;
            set => Set(ref _caption, value);
        }

        public List<UserCollection> Collections { get; set; } = new ElibContext(ApplicationSettings.GetInstance().DatabasePath).UserCollections.ToList();

        private bool _isPaneOpen = true;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set => Set(ref _isPaneOpen, true);
        }
    }
}
