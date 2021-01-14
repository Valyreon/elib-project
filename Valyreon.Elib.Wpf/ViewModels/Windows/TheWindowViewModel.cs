using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Valyreon.Elib.Mvvm;using MahApps.Metro.Controls.Dialogs;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.ViewModels.Controls;
using Valyreon.Elib.Wpf.ViewModels.Flyouts;

namespace Valyreon.Elib.Wpf.ViewModels.Windows
{
    public class TheWindowViewModel : ViewModelBase
    {
        private object flyoutControl;
        private bool isBookDetailsFlyoutOpen;
        private ITabViewModel selectedTab;

        public TheWindowViewModel()
        {
            MessengerInstance.Register<ShowBookDetailsMessage>(this, HandleBookFlyout);
            MessengerInstance.Register(this, (ShowDialogMessage m) => ShowDialog(m.Title, m.Text));
            MessengerInstance.Register<ShowInputDialogMessage>(this, HandleInputDialog);
            MessengerInstance.Register(this, (CloseFlyoutMessage _) =>
            {
                IsBookDetailsFlyoutOpen = false;
                FlyoutControl = null;
            });
            MessengerInstance.Register(this, (OpenAddBooksFormMessage m) => HandleAddBooksFlyout(m.BooksToAdd));
            MessengerInstance.Register(this, (EditBookMessage m) => HandleEditBookFlyout(m.Book));

            Tabs = new ObservableCollection<ITabViewModel>
            {
                new BooksTabViewModel(),
                new QuotesTabViewModel(),
                new SettingsTabViewModel()
            };
            SelectedTab = Tabs[0];
        }

        public ICommand CloseDetailsCommand => new RelayCommand(() =>
        {
            IsBookDetailsFlyoutOpen = false;
            FlyoutControl = null;
        });

        public ICommand EscKeyCommand => new RelayCommand(ProcessEscKey);

        public object FlyoutControl
        {
            get => flyoutControl;
            set => Set(() => FlyoutControl, ref flyoutControl, value);
        }

        public bool IsBookDetailsFlyoutOpen
        {
            get => isBookDetailsFlyoutOpen;
            set => Set(() => IsBookDetailsFlyoutOpen, ref isBookDetailsFlyoutOpen, value);
        }

        public ITabViewModel SelectedTab
        {
            get => selectedTab;
            set => Set(() => SelectedTab, ref selectedTab, value);
        }

        public ObservableCollection<ITabViewModel> Tabs { get; set; }

        private async void HandleInputDialog(ShowInputDialogMessage obj)
        {
            var input = await DialogCoordinator.Instance.ShowInputAsync(this, obj.Title, obj.Text);
            obj.CallOnResult(input);
        }

        private void HandleEditBookFlyout(Book book)
        {
            FlyoutControl = new EditBookViewModel(book);
            IsBookDetailsFlyoutOpen = true;
        }

        private void HandleAddBooksFlyout(IList<Book> booksToAdd)
        {
            FlyoutControl = new AddNewBooksViewModel(booksToAdd);
            IsBookDetailsFlyoutOpen = true;
        }

        private async void ShowDialog(string title, string text)
        {
            //await ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync(title, text);
            await DialogCoordinator.Instance.ShowMessageAsync(this, title, text);
        }

        private void ProcessEscKey()
        {
            if (IsBookDetailsFlyoutOpen)
            {
                IsBookDetailsFlyoutOpen = false;
                FlyoutControl = null;
            }
        }

        private void HandleBookFlyout(ShowBookDetailsMessage obj)
        {
            FlyoutControl = new BookDetailsViewModel(obj.Book);
            IsBookDetailsFlyoutOpen = true;
        }
    }
}
