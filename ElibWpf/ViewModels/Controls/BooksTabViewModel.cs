using Domain;
using ElibWpf.BindingItems;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Controls
{
    public class BooksTabViewModel : ViewModelBase, IPageViewModel
    {
        private string caption = "Books";
        private IViewer currentViewer;
        private UserCollection selectedCollection;
        private PaneMainItem selectedMainPaneItem;
        public BooksTabViewModel()
        {
            MainPaneItems = new ObservableCollection<PaneMainItem>
            {
                new PaneMainItem("All", "Book", "All Books", (Book x) => true),
                new PaneMainItem("Favorite", "Star", "Favorite Books", (Book x) => x.IsFavorite),
                new PaneMainItem("Read", "Check", "Read Books", (Book x) => x.IsRead),
                new PaneMainItem("Not Read", "TimesCircle", "Not Read Books", (Book x) => !x.IsRead)
            };
            SelectedMainPaneItem = MainPaneItems[0];
            PaneSelectionChanged();
        }

        public string Caption
        {
            get => caption;
            set => Set(ref caption, value);
        }

        public List<UserCollection> Collections { get; set; } = App.Database.UserCollections.ToList();
        public IViewer CurrentViewer
        {
            get => currentViewer;
            set => Set(ref currentViewer, value);
        }

        public ObservableCollection<PaneMainItem> MainPaneItems { get; set; }
        public ICommand PaneSelectionChangedCommand { get => new RelayCommand(this.PaneSelectionChanged); }

        public ICommand RefreshCommand { get => new RelayCommand(this.RefreshCurrent); }

        public UserCollection SelectedCollection
        {
            get => selectedCollection;

            set
            {
                Set(ref selectedCollection, value);
                if (selectedCollection != null)
                {
                    var newViewer = new BookViewerViewModel($"Collection {selectedCollection.Tag}", (Book x) => x.UserCollections.Where(c => c.Id == SelectedCollection.Id).Count() > 0);
                    CurrentViewer = newViewer;
                }
            }
        }

        public PaneMainItem SelectedMainPaneItem
        {
            get => selectedMainPaneItem;
            set => Set("SelectedMainPaneItem", ref selectedMainPaneItem, value);
        }
        private void PaneSelectionChanged()
        {
            if (SelectedMainPaneItem != null)
            {
                var newViewer = new BookViewerViewModel(SelectedMainPaneItem.ViewerCaption, SelectedMainPaneItem.Condition);
                CurrentViewer = newViewer;
            }
        }

        private void RefreshCurrent()
        {
            var newViewer = CurrentViewer.Clone() as IViewer;
            CurrentViewer = newViewer;
        }
    }
}
