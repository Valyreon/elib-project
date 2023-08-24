using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Extensions;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.Services;
using Valyreon.Elib.Wpf.ViewModels.Controls;
using Valyreon.Elib.Wpf.ViewModels.Dialogs;
using Valyreon.Elib.Wpf.ViewModels.Flyouts;
using Valyreon.Elib.Wpf.Views.Dialogs;
using Timer = System.Timers.Timer;

namespace Valyreon.Elib.Wpf.ViewModels.Windows
{
    public class TheWindowViewModel : ViewModelBase, IDisposable
    {
        private object flyoutControl;
        private bool isBookDetailsFlyoutOpen;
        private ITabViewModel selectedTab;

        private readonly Queue<ShowNotificationMessage> messages = new();
        private readonly Timer notificationTimer = new();
        private ElibFileSystemWatcher fileSystemWatcher = new ElibFileSystemWatcher();

        public ShowNotificationMessage CurrentNotificationMessage
        {
            get => currentNotificationMessage;
            set => Set(() => CurrentNotificationMessage, ref currentNotificationMessage, value);
        }

        public ICommand NextNotificationCommand => new RelayCommand(() => HandleNextNotification());

        public TheWindowViewModel()
        {
            notificationTimer.Interval = 3000;
            notificationTimer.Elapsed += HandleNextNotification;

            MessengerInstance.Register<AppSettingsChangedMessage>(this, _ =>
            {
                fileSystemWatcher.Dispose();
                fileSystemWatcher = new ElibFileSystemWatcher();
            });
            MessengerInstance.Register<ShowBookDetailsMessage>(this, HandleBookFlyout);
            MessengerInstance.Register<ShowNotificationMessage>(this, HandleShowNotification);
            MessengerInstance.Register(this, (ShowDialogMessage m) => ShowDialog(m.Title, m.Text));
            MessengerInstance.Register<ShowInputDialogMessage>(this, HandleInputDialog);
            MessengerInstance.Register(this, (CloseFlyoutMessage _) =>
            {
                IsBookDetailsFlyoutOpen = false;
                FlyoutControl = null;
            });
            MessengerInstance.Register(this, (OpenAddBooksFormMessage m) => HandleAddBooksFlyout(m.BooksToAdd));
            MessengerInstance.Register(this, (EditBookMessage m) => HandleEditBookFlyout(m.Book));
            MessengerInstance.Register(this, (ScanForNewBooksMessage m) => HandleScanForNewBooks());

            Tabs = new ObservableCollection<ITabViewModel>
            {
                new BooksTabViewModel(),
                new QuotesTabViewModel(),
            };
            SelectedTab = Tabs[0];
        }

        private void HandleNextNotification(object sender = null, ElapsedEventArgs e = null)
        {
            if (messages.Count > 0)
            {
                CurrentNotificationMessage = messages.Dequeue();
                return;
            }

            notificationTimer.Stop();
            CurrentNotificationMessage = null;
        }

        private void HandleShowNotification(ShowNotificationMessage message)
        {
            if (notificationTimer.Enabled)
            {
                messages.Enqueue(message);
                return;
            }

            CurrentNotificationMessage = message;
            notificationTimer.Start();
        }

        public ICommand CloseDetailsCommand => new RelayCommand(() =>
        {
            IsBookDetailsFlyoutOpen = false;
            FlyoutControl = null;
        });

        public ICommand EscKeyCommand => new RelayCommand(ProcessEscKey);
        public ICommand OpenSettingsCommand => new RelayCommand(HandleOpenSettings);
        public ICommand ScanForNewContentCommand => new RelayCommand(HandleScanForNewBooks);
        public ICommand RefreshViewCommand => new RelayCommand(HandleRefreshView);

        private static DateTime lastRefresh = DateTime.MinValue;
        private ShowNotificationMessage currentNotificationMessage;
        private static readonly TimeSpan refreshPause = new TimeSpan(0, 0, 0, 0, 500);
        public void HandleRefreshView()
        {
            if (DateTime.Now - lastRefresh > refreshPause)
            {
                MessengerInstance.Send(new RefreshCurrentViewMessage());
                lastRefresh = DateTime.Now;
            }
        }

        private async void HandleOpenSettings()
        {
            var dialog = new ApplicationSettingsDialog
            {
                DataContext = new ApplicationSettingsDialogViewModel()
            };
            await DialogCoordinator.Instance.ShowMetroDialogAsync(Application.Current.MainWindow.DataContext, dialog);
        }

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
            var input = await DialogCoordinator.Instance.ShowInputAsync(Application.Current.MainWindow.DataContext, obj.Title, obj.Text);
            obj.CallOnResult(input);
        }

        private async void HandleScanForNewBooks()
        {
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var importer = new ImportService(uow);
            var newBookPaths = (await importer.ImportAsync()).ToList();

            if (!newBookPaths.Any())
            {
                return;
            }

            HandleAddBooksFlyout(newBookPaths);
        }

        private void HandleEditBookFlyout(Book book)
        {
            FlyoutControl = new EditBookViewModel(book);
            IsBookDetailsFlyoutOpen = true;
        }

        private void HandleAddBooksFlyout(IList<string> booksToAdd)
        {
            FlyoutControl = new AddNewBooksViewModel(booksToAdd);
            IsBookDetailsFlyoutOpen = true;
        }

        private async void ShowDialog(string title, string text)
        {
            //await ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync(title, text);
            await DialogCoordinator.Instance.ShowMessageAsync(Application.Current.MainWindow.DataContext, title, text);
        }

        private async void ProcessEscKey()
        {
            var currentDialog = await DialogCoordinator.Instance.GetCurrentDialogAsync<BaseMetroDialog>(Application.Current.MainWindow.DataContext);
            if (IsBookDetailsFlyoutOpen && currentDialog == null)
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

        public void Dispose()
        {
            fileSystemWatcher.Dispose();
            MessengerInstance.Unregister(this);
        }
    }
}
