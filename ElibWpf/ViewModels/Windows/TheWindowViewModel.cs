using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DataLayer;
using Domain;
using ElibWpf.Messages;
using ElibWpf.ViewModels.Controls;
using ElibWpf.ViewModels.Flyouts;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using Models;
using Models.Observables;

namespace ElibWpf.ViewModels.Windows
{
    public class TheWindowViewModel : ViewModelBase
    {
        private object flyoutControl;
        private bool isBookDetailsFlyoutOpen;
        private ITabViewModel selectedTab;

        public TheWindowViewModel()
        {
            this.MessengerInstance.Register<ShowBookDetailsMessage>(this, this.HandleBookFlyout);
            this.MessengerInstance.Register(this, (ShowDialogMessage m) => this.ShowDialog(m.Title, m.Text));
            this.MessengerInstance.Register<ShowInputDialogMessage>(this, this.HandleInputDialog);
            this.MessengerInstance.Register(this, (CloseFlyoutMessage m) =>
            {
                this.IsBookDetailsFlyoutOpen = false;
                this.FlyoutControl = null;
            });
            this.MessengerInstance.Register(this, (OpenAddBooksFormMessage m) => { this.HandleAddBooksFlyout(m.BooksToAdd); });
            this.MessengerInstance.Register(this, (EditBookMessage m) => { this.HandleEditBookFlyout(m.Book); });

            this.Tabs = new ObservableCollection<ITabViewModel>
            {
                new BooksTabViewModel(),
                new QuotesTabViewModel(),
                new SettingsTabViewModel()
            };
            this.SelectedTab = this.Tabs[0];
        }

        public ICommand CloseDetailsCommand => new RelayCommand(() =>
        {
            this.IsBookDetailsFlyoutOpen = false;
            this.FlyoutControl = null;
        });

        public ICommand EscKeyCommand => new RelayCommand(this.ProcessEscKey);

        public object FlyoutControl
        {
            get => this.flyoutControl;
            set => this.Set(ref this.flyoutControl, value);
        }

        public bool IsBookDetailsFlyoutOpen
        {
            get => this.isBookDetailsFlyoutOpen;
            set => this.Set(ref this.isBookDetailsFlyoutOpen, value);
        }

        public ITabViewModel SelectedTab
        {
            get => this.selectedTab;
            set => this.Set(ref this.selectedTab, value);
        }

        public ObservableCollection<ITabViewModel> Tabs { get; set; }

        private async void HandleInputDialog(ShowInputDialogMessage obj)
        {
            string input = await DialogCoordinator.Instance.ShowInputAsync(this, obj.Title, obj.Text);
            obj.CallOnResult(input);
        }

        private void HandleEditBookFlyout(ObservableBook book)
        {
            this.FlyoutControl = new EditBookViewModel(book);
            this.IsBookDetailsFlyoutOpen = true;
        }

        private void HandleAddBooksFlyout(IList<Book> booksToAdd)
        {
            this.FlyoutControl = new AddNewBooksViewModel(booksToAdd);
            this.IsBookDetailsFlyoutOpen = true;
        }

        private async void ShowDialog(string title, string text)
        {
            //await ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync(title, text);
            await DialogCoordinator.Instance.ShowMessageAsync(this, title, text);
        }

        private void ProcessEscKey()
        {
            if (this.IsBookDetailsFlyoutOpen)
            {
                this.IsBookDetailsFlyoutOpen = false;
                this.FlyoutControl = null;
            }
            else
            {
                this.MessengerInstance.Send(new GoBackMessage());
            }
        }

        private async void HandleBookFlyout(ShowBookDetailsMessage obj)
        {
            using ElibContext context = ApplicationSettings.CreateContext();
            context.Books.Attach(obj.Book.Book);
            await context.Entry(obj.Book.Book).Collection(b => b.UserCollections).LoadAsync();
            this.FlyoutControl = new BookDetailsViewModel(obj.Book);
            this.IsBookDetailsFlyoutOpen = true;
        }
    }
}