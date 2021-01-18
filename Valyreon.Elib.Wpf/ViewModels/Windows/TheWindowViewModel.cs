using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Controls;
using Valyreon.Elib.Wpf.Interfaces;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.ViewModels.Controls;
using Valyreon.Elib.Wpf.ViewModels.Flyouts;
using Valyreon.Elib.Wpf.Views.Flyouts;

namespace Valyreon.Elib.Wpf.ViewModels.Windows
{
    public class TheWindowViewModel : ViewModelBase
    {
        private ITabViewModel selectedTab;

        public TheWindowViewModel()
        {
            MessengerInstance.Register<ShowBookDetailsMessage>(this, HandleBookFlyout);
            MessengerInstance.Register(this, (ShowDialogMessage m) => ShowDialog(m.Title, m.Text));
            MessengerInstance.Register<ShowInputDialogMessage>(this, HandleInputDialog);
            MessengerInstance.Register(this, (CloseFlyoutMessage _) =>
            {
                FlyoutControl.IsOpen = false;
                FlyoutControl.ContentControl = null;
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

            FlyoutControl = new FlyoutPanel();
        }

        public ICommand CloseDetailsCommand => new RelayCommand(() =>
        {
            FlyoutControl.IsOpen = false;
            FlyoutControl.ContentControl = null;
        });

        public ICommand EscKeyCommand => new RelayCommand(ProcessEscKey);

        public IFlyoutPanel FlyoutControl { get; }

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
            FlyoutControl.ContentControl = new EditBookViewModel(book);
            FlyoutControl.IsOpen = true;
        }

        private void HandleAddBooksFlyout(IList<Book> booksToAdd)
        {
            FlyoutControl.ContentControl = new AddNewBooksViewModel(booksToAdd);
            FlyoutControl.IsOpen = true;
        }

        private async void ShowDialog(string title, string text)
        {
            await DialogCoordinator.Instance.ShowMessageAsync(this, title, text);
        }

        private void ProcessEscKey()
        {
            if (FlyoutControl.IsOpen)
            {
                FlyoutControl.IsOpen = false;
                FlyoutControl.ContentControl = null;
            }
        }

        private void HandleBookFlyout(ShowBookDetailsMessage obj)
        {
            FlyoutControl.ContentControl = new BookDetailsControl
            {
                DataContext = new BookDetailsViewModel(obj.Book),
            };
            FlyoutControl.IsOpen = true;
        }
    }
}
