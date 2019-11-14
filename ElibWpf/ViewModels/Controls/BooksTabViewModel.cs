using Domain;
using ElibWpf.BindingItems;
using ElibWpf.DataStructures;
using ElibWpf.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Controls
{
    public class BooksTabViewModel : ViewModelBase, ITabViewModel
    {
        private string caption = "Books";
        private IViewer currentViewer;
        private UserCollection selectedCollection;
        private PaneMainItem selectedMainPaneItem;
        private readonly ObservableStack<IViewer> viewerHistory = new ObservableStack<IViewer>();

        public BooksTabViewModel()
        {
            MessengerInstance.Register<AuthorSelectedMessage>(this, this.HandleAuthorSelection);
            MessengerInstance.Register<SeriesSelectedMessage>(this, this.HandleSeriesSelection);

            viewerHistory.AddHandlerOnStackChange((object sender, NotifyCollectionChangedEventArgs e) => RaisePropertyChanged("IsBackEnabled"));

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

        private void HandleSeriesSelection(SeriesSelectedMessage obj)
        {
            string viewerCaption = $"{obj.Series.Name} Series";
            if(viewerCaption != CurrentViewer.Caption)
            {
                viewerHistory.Push(CurrentViewer);
                CurrentViewer = new BookViewerViewModel(viewerCaption, (Book x) => x.SeriesId.HasValue && x.SeriesId == obj.Series.Id);
            }
        }

        private void HandleAuthorSelection(AuthorSelectedMessage obj)
        {
            string viewerCaption = $"Books by {obj.Author.Name}";
            if (viewerCaption != CurrentViewer.Caption)
            {
                viewerHistory.Push(CurrentViewer);
                CurrentViewer = new BookViewerViewModel(viewerCaption, (Book x) => x.Authors.Select(a => a.Id).Contains(obj.Author.Id));
            }
        }

        public ICommand BackCommand { get => new RelayCommand(this.GoToPreviousViewer); }

        public bool IsBackEnabled { get => viewerHistory.Count > 0; }

        private void GoToPreviousViewer()
        {
            CurrentViewer = viewerHistory.Pop();
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
                    viewerHistory.Clear();
                    CurrentViewer = new BookViewerViewModel($"Collection {selectedCollection.Tag}", (Book x) => x.UserCollections.Where(c => c.Id == SelectedCollection.Id).Count() > 0);
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
                viewerHistory.Clear();
                CurrentViewer = new BookViewerViewModel(SelectedMainPaneItem.ViewerCaption, SelectedMainPaneItem.Condition);
            }
        }

        private void RefreshCurrent()
        {
            CurrentViewer = CurrentViewer.Clone() as IViewer;
        }
    }
}
