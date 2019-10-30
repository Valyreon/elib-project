using Domain;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Controls
{
    public class BooksTabViewModel : ViewModelBase, IPageViewModel
    {
        private BookViewerViewModel AllBooksControl { get; set; } = new BookViewerViewModel($"All Books", (Book x) => true);
        private BookViewerViewModel FavoriteBooksControl { get; set; } = new BookViewerViewModel($"All Books", (Book x) => x.IsFavorite);

        public BooksTabViewModel()
        {
            CurrentViewer = AllBooksControl;
        }

        private string caption = "Books";
        public string Caption
        {
            get => caption;
            set => Set(ref caption, value);
        }

        public List<UserCollection> Collections { get; set; } = App.Database.UserCollections.ToList();

        private UserCollection selectedCollection;
        public UserCollection SelectedCollection
        {
            get => selectedCollection;

            set
            {
                Set(ref selectedCollection, value);
                if (selectedCollection != null)
                {
                    CurrentViewer = new BookViewerViewModel($"Collection {selectedCollection.Tag}", (Book x) => x.UserCollections.Where(c => c.Id == SelectedCollection.Id).Count() > 0);
                }
            }
        }

        private ISearchable currentViewer;
        public ISearchable CurrentViewer
        {
            get => currentViewer;
            set => Set(ref currentViewer, value);
        }

        public ICommand ShowAllBooksCommand { get => new RelayCommand(() => { CurrentViewer = AllBooksControl; }); }

        public ICommand ShowFavoriteBooksCommand { get => new RelayCommand(() => { CurrentViewer = FavoriteBooksControl; }); }
    }
}
