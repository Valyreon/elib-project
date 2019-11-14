using ElibWpf.Messages;
using ElibWpf.ViewModels.Controls;
using ElibWpf.ViewModels.Flyouts;
using ElibWpf.Views;
using ElibWpf.Views.Flyouts;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Windows
{
    public class TheWindowViewModel : ViewModelBase
    {
        private bool isBookDetailsFlyoutOpen;
        public bool IsBookDetailsFlyoutOpen
        {
            get => isBookDetailsFlyoutOpen;
            set => Set(ref isBookDetailsFlyoutOpen, value);
        }

        public ICommand CloseDetailsCommand { get => new RelayCommand(() => { IsBookDetailsFlyoutOpen = false; FlyoutControl = null; }); }

        public TheWindowViewModel()
        {
            MessengerInstance.Register<ShowBookDetailsMessage>(this, this.HandleBookFlyout);

            var books = new BooksTabViewModel();
            var quotes = new QuotesTabViewModel();
            var settings = new SettingsTabViewModel();
            Pages = new ObservableCollection<IPageViewModel>
            {
                books,
                quotes,
                settings
            };
            SelectedPage = Pages[0];
        }

        private void HandleBookFlyout(ShowBookDetailsMessage obj)
        {
            IsBookDetailsFlyoutOpen = true;
            FlyoutControl = new BookDetailsViewModel(obj.Book);
        }

        private ObservableCollection<IPageViewModel> _pages = new ObservableCollection<IPageViewModel>();
        public ObservableCollection<IPageViewModel> Pages
        {
            get => _pages;
            private set { Set(() => Pages, ref _pages, value); }
        }

        private IPageViewModel selectedPage;
        public IPageViewModel SelectedPage
        {
            get => selectedPage;
            set => Set(ref selectedPage, value);
        }

        private object flyoutControl;
        public object FlyoutControl
        {
            get => this.flyoutControl;
            set => Set("FlyoutControl", ref flyoutControl, value);
        }
    }
}