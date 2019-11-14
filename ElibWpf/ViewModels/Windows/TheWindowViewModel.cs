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

            Pages = new ObservableCollection<ITabViewModel>
            {
                new BooksTabViewModel(),
                new QuotesTabViewModel(),
                new SettingsTabViewModel()
            };
            SelectedPage = Pages[0];
        }

        private void HandleBookFlyout(ShowBookDetailsMessage obj)
        {
            FlyoutControl = new BookDetailsViewModel(obj.Book);
            IsBookDetailsFlyoutOpen = true;
        }

        private ObservableCollection<ITabViewModel> _pages = new ObservableCollection<ITabViewModel>();
        public ObservableCollection<ITabViewModel> Pages
        {
            get => _pages;
            private set { Set(() => Pages, ref _pages, value); }
        }

        private ITabViewModel selectedPage;
        public ITabViewModel SelectedPage
        {
            get => selectedPage;
            set => Set(ref selectedPage, value);
        }

        private object flyoutControl;
        public object FlyoutControl
        {
            get => this.flyoutControl;
            set => Set(ref flyoutControl, value);
        }
    }
}