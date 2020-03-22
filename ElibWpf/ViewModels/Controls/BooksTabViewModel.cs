﻿using Domain;

using ElibWpf.BindingItems;
using ElibWpf.Converters;
using ElibWpf.DataStructures;
using ElibWpf.Messages;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;

namespace ElibWpf.ViewModels.Controls
{
    public class BooksTabViewModel : ViewModelBase, ITabViewModel
    {
        private readonly ObservableStack<IViewer> viewerHistory = new ObservableStack<IViewer>();
        private string caption = "Books";
        private IViewer currentViewer;
        private UserCollection selectedCollection;
        private PaneMainItem selectedMainPaneItem;
        private string searchInput;
        private bool isInSearchResults = false;

        public BooksTabViewModel()
        {
            MessengerInstance.Register<AuthorSelectedMessage>(this, this.HandleAuthorSelection);
            MessengerInstance.Register<SeriesSelectedMessage>(this, this.HandleSeriesSelection);
            MessengerInstance.Register<CollectionSelectedMessage>(this, this.HandleCollectionSelection);
            MessengerInstance.Register<GoBackMessage>(this, x => this.GoToPreviousViewer());
            MessengerInstance.Register<RefreshSidePaneCollectionsMessage>(this, async x =>  { this.Collections = await Task.Run(() => App.Database.UserCollections.ToList()); RaisePropertyChanged("Collections"); });

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
            SearchInputText = "";
        }

        private void HandleCollectionSelection(CollectionSelectedMessage message)
        {
            SelectedCollection = Collections.Where(c => c.Id == message.Collection.Id).FirstOrDefault();
        }

        public ICommand BackCommand { get => new RelayCommand(this.GoToPreviousViewer); }

        public ICommand SearchInputChangedCommand { get => new RelayCommand(this.ProcessSearchInput); }

        public ICommand SearchCommand { get => new RelayCommand(this.ProcessSearchInput); }

        private async void ProcessSearchInput()
        {
            if (!string.IsNullOrWhiteSpace(searchInput))
            {
                if (!isInSearchResults)
                {
                    viewerHistory.Push(CurrentViewer);
                    isInSearchResults = true;
                }
                Func<Book, bool> condition = (Book x) => x.Title.Contains(searchInput) && viewerHistory.Peek().DefaultCondition(x);
                int temp = await Task.Run(() => App.Database.Books.Where(condition).Count());


                if (temp > 0)
                    CurrentViewer = new BookViewerViewModel($"Search results for '{searchInput}' in " + viewerHistory.Peek().Caption, condition);
                else
                    MessengerInstance.Send(new ShowDialogMessage("No matches", "No books found matching the search conditions."));
            }
        }

        public string Caption
        {
            get => caption;
            set => Set(ref caption, value);
        }

        public string SearchInputText
        {
            get => searchInput;
            set 
            {
                Set("SearchInputText", ref searchInput, value);
            }
        }

        public List<UserCollection> Collections { get; set; } = App.Database.UserCollections.ToList();

        public IViewer CurrentViewer
        {
            get => currentViewer;
            set => Set(ref currentViewer, value);
        }

        public bool IsBackEnabled { get => viewerHistory.Count > 0; }

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

        private void GoToPreviousViewer()
        {
            if (IsBackEnabled)
            {
                isInSearchResults = false;
                CurrentViewer = viewerHistory.Pop();
            }
        }

        private void HandleAuthorSelection(AuthorSelectedMessage obj)
        {
            string viewerCaption = $"Books by {obj.Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j)}";
            if (viewerCaption != CurrentViewer.Caption)
            {
                viewerHistory.Push(CurrentViewer);
                Func<Book, bool> selector = (Book x) =>
                {
                    var bookAuthorsIds = x.Authors.Select(a => a.Id);
                    foreach (Author selected in obj.Authors)
                    {
                        if (!bookAuthorsIds.Contains(selected.Id))
                        {
                            return false;
                        }
                    }
                    return true;
                };
                CurrentViewer = new BookViewerViewModel(viewerCaption, selector);
            }
        }

        private void HandleSeriesSelection(SeriesSelectedMessage obj)
        {
            string viewerCaption = $"{obj.Series.Name} Series";
            if (viewerCaption != CurrentViewer.Caption)
            {
                viewerHistory.Push(CurrentViewer);
                CurrentViewer = new BookViewerViewModel(viewerCaption, (Book x) => x.SeriesId.HasValue && x.SeriesId == obj.Series.Id);
            }
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
            if (SelectedCollection == null) SelectedCollection = Collections[0];
            CurrentViewer = CurrentViewer.Clone() as IViewer;
        }
    }
}