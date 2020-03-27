using Domain;
using ElibWpf.Messages;
using ElibWpf.ViewModels.Controls;
using ElibWpf.ViewModels.Flyouts;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Windows
{
    public class TheWindowViewModel : ViewModelBase
    {
        private object flyoutControl;
        private bool isBookDetailsFlyoutOpen;
        private ITabViewModel selectedTab;

        public TheWindowViewModel()
        {
            MessengerInstance.Register<ShowBookDetailsMessage>(this, this.HandleBookFlyout);
            MessengerInstance.Register(this, (ShowDialogMessage m) => ShowDialog(m.Title, m.Text));
            MessengerInstance.Register(this, (CloseFlyoutMessage m) => { IsBookDetailsFlyoutOpen = false; FlyoutControl = null; });
            MessengerInstance.Register(this, (OpenAddBooksFormMessage m) => { this.HandleAddBooksFlyout(m.BooksToAdd); });
            MessengerInstance.Register(this, (EditBookMessage m) => { this.HandleEditBookFlyout(m.Book); });

            Tabs = new ObservableCollection<ITabViewModel>
            {
                new BooksTabViewModel(),
                new QuotesTabViewModel(),
                new SettingsTabViewModel()
            };
            SelectedTab = Tabs[0];
        }

        private void HandleEditBookFlyout(Book book)
        {
            FlyoutControl = new EditBookViewModel(book);
            IsBookDetailsFlyoutOpen = true;
        }

        private void HandleAddBooksFlyout(IEnumerable<Book> booksToAdd)
        {
            FlyoutControl = new AddNewBooksViewModel(booksToAdd);
            IsBookDetailsFlyoutOpen = true;
        }

        private async void ShowDialog(string title, string text)
        {
            await ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync(title, text);
        }

        public ICommand CloseDetailsCommand { get => new RelayCommand(() => { IsBookDetailsFlyoutOpen = false; FlyoutControl = null; }); }

        public ICommand EscKeyCommand { get => new RelayCommand(ProcessEscKey); }

        private void ProcessEscKey()
        {
            if (IsBookDetailsFlyoutOpen)
            {
                IsBookDetailsFlyoutOpen = false; FlyoutControl = null;
            }
            else
            {
                MessengerInstance.Send(new GoBackMessage());
            }
        }

        public object FlyoutControl
        {
            get => this.flyoutControl;
            set => Set(ref flyoutControl, value);
        }

        public bool IsBookDetailsFlyoutOpen
        {
            get => isBookDetailsFlyoutOpen;
            set => Set(ref isBookDetailsFlyoutOpen, value);
        }

        public ITabViewModel SelectedTab
        {
            get => selectedTab;
            set => Set(ref selectedTab, value);
        }

        public ObservableCollection<ITabViewModel> Tabs { get; set; }

        private void HandleBookFlyout(ShowBookDetailsMessage obj)
        {
            FlyoutControl = new BookDetailsViewModel(obj.Book);
            IsBookDetailsFlyoutOpen = true;
        }
    }
}