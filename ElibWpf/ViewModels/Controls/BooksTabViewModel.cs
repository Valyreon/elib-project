using Domain;
using ElibWpf.Components;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Controls
{
    public class BooksTabViewModel : ViewModelBase, IPageViewModel
    {
        public ObservableCollection<PaneMainItem> MainPaneItems { get; set; }

        private List<Book> allBooks;

        private PaneMainItem selectedMainPaneItem;
        public PaneMainItem SelectedMainPaneItem
        {
            get => selectedMainPaneItem;
            set => Set("SelectedMainPaneItem", ref selectedMainPaneItem, value);
        }

        public BooksTabViewModel()
        {
            MainPaneItems = new ObservableCollection<PaneMainItem>
            {
                new PaneMainItem("All", "Book", new BookViewerViewModel($"All Books", (Book x) => true)),
                new PaneMainItem("Favorite", "Star", new BookViewerViewModel($"Favorite Books", (Book x) => x.IsFavorite)),
                new PaneMainItem("Read", "Check", new BookViewerViewModel($"Read Books", (Book x) => x.IsRead)),
                new PaneMainItem("Not Read", "TimesCircle", new BookViewerViewModel($"Unread Books", (Book x) => !x.IsRead))
            };
            SelectedMainPaneItem = MainPaneItems[0];
            PaneSelectionChanged();
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
                    var newViewModel = new BookViewerViewModel($"Collection {selectedCollection.Tag}", (Book x) => x.UserCollections.Where(c => c.Id == SelectedCollection.Id).Count() > 0);
                    CurrentViewer = newViewModel;
                    foreach (var book in allBooks.Where(newViewModel.DefaultCondition))
                    {
                        App.Current.Dispatcher.Invoke(() => newViewModel.Books.Add(book));
                    }
                }
            }
        }

        private ISearchable currentViewer;
        public ISearchable CurrentViewer
        {
            get => currentViewer;
            set => Set(ref currentViewer, value);
        }

        public ICommand PaneSelectionChangedCommand { get => new RelayCommand(this.PaneSelectionChanged); }

        private void PaneSelectionChanged()
        {
            if (SelectedMainPaneItem != null)
            {
                CurrentViewer = SelectedMainPaneItem.ViewModel;
            }
        }

        public ICommand OnPaneLoadedCommand { get => new RelayCommand(this.Refresh); }

        private async void Refresh()
        {
            allBooks = await Task.Run(() => App.Database.Books.Include("Authors").Include("Series").Include("UserCollections").ToList());
            foreach(var item in MainPaneItems)
            {
                App.Current.Dispatcher.Invoke(() => item.ViewModel.Books.Clear());
                foreach(var book in allBooks.Where(item.ViewModel.DefaultCondition))
                {
                    App.Current.Dispatcher.Invoke(() => item.ViewModel.Books.Add(book));
                }
            };
        }
    }
}
