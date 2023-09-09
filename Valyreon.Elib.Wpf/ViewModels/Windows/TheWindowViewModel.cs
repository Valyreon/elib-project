using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Interfaces;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.Services;
using Valyreon.Elib.Wpf.Themes.CustomComponents.Controls;
using Valyreon.Elib.Wpf.ViewModels.Controls;
using Valyreon.Elib.Wpf.ViewModels.Flyouts;
using Timer = System.Timers.Timer;

namespace Valyreon.Elib.Wpf.ViewModels.Windows
{
    public class TheWindowViewModel : ViewModelBase, IDisposable
    {
        private IFlyoutPanel flyoutControl;
        private ITabViewModel selectedTab;
        private readonly ApplicationProperties applicationProperties = ApplicationData.GetProperties();

        private readonly Queue<ShowNotificationMessage> messages = new();
        private readonly Timer notificationTimer = new();
        private ElibFileSystemWatcher fileSystemWatcher;

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

            fileSystemWatcher = new(applicationProperties);

            MessengerInstance.Register<AppSettingsChangedMessage>(this, _ =>
            {
                fileSystemWatcher.Dispose();
                fileSystemWatcher = new ElibFileSystemWatcher(applicationProperties);
            });
            MessengerInstance.Register<OpenFlyoutMessage>(this, m => HandleOpenFlyout(m.ViewModel));
            MessengerInstance.Register<OpenBookDetailsFlyoutMessage>(this, m => HandleOpenFlyout(new BookDetailsViewModel(m.Book, applicationProperties)));
            MessengerInstance.Register<ShowNotificationMessage>(this, HandleShowNotification);
            MessengerInstance.Register(this, (ShowDialogMessage m) =>
            {
                if (!IsDialogOpen)
                {
                    DialogControl = m.ViewModel;
                    IsDialogOpen = true;
                }
            });
            MessengerInstance.Register(this, (CloseDialogMessage m) => IsDialogOpen = false);
            MessengerInstance.Register(this, (CloseFlyoutMessage _) => CloseFlyoutPanel());
            MessengerInstance.Register(this, (OpenAddBooksFormMessage m) => HandleAddBooksFlyout(m.BooksToAdd));
            MessengerInstance.Register(this, (EditBookMessage m) => HandleEditBookFlyout(m.Book));
            MessengerInstance.Register(this, (ScanForNewBooksMessage m) => HandleScanForNewBooks());

            Tabs = new ObservableCollection<ITabViewModel>
            {
                new BooksTabViewModel(applicationProperties),
                new QuotesTabViewModel(),
                new ApplicationSettingsViewModel(applicationProperties)
            };
            SelectedTab = Tabs[0];

            FlyoutControl = new FlyoutPanel();
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

        public void OpenFlyoutPanel(object content)
        {
            flyoutControl.ContentControl = content;
            flyoutControl.IsOpen = true;
        }

        public void CloseFlyoutPanel()
        {
            flyoutControl.IsOpen = false;
            flyoutControl.ContentControl = null;
        }

        public ICommand EscKeyCommand => new RelayCommand(ProcessEscKey);
        public ICommand CloseFlyoutCommand => new RelayCommand(() => FlyoutControl = null);
        public ICommand ScanForNewContentCommand => new RelayCommand(HandleScanForNewBooks);
        public ICommand RefreshViewCommand => new RelayCommand(HandleRefreshView);

        private static DateTime lastRefresh = DateTime.MinValue;
        private ShowNotificationMessage currentNotificationMessage;
        private DialogViewModel dialogControl;
        private bool isDialogOpen;
        private static readonly TimeSpan refreshPause = new TimeSpan(0, 0, 0, 0, 500);

        public void HandleRefreshView()
        {
            if (DateTime.Now - lastRefresh > refreshPause)
            {
                MessengerInstance.Send(new RefreshCurrentViewMessage());
                lastRefresh = DateTime.Now;
            }
        }

        public IFlyoutPanel FlyoutControl
        {
            get => flyoutControl;
            set => Set(() => FlyoutControl, ref flyoutControl, value);
        }

        public DialogViewModel DialogControl
        {
            get => dialogControl;
            set => Set(() => DialogControl, ref dialogControl, value);
        }

        public bool IsDialogOpen
        {
            get => isDialogOpen;
            set => Set(() => IsDialogOpen, ref isDialogOpen, value);
        }

        public ITabViewModel SelectedTab
        {
            get => selectedTab;
            set => Set(() => SelectedTab, ref selectedTab, value);
        }

        public ObservableCollection<ITabViewModel> Tabs { get; set; }

        private async void HandleScanForNewBooks()
        {
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var importer = new ImportService(uow, applicationProperties);
            var newBookPaths = (await importer.ImportAsync()).ToList();

            if (!newBookPaths.Any())
            {
                return;
            }

            HandleAddBooksFlyout(newBookPaths);
        }

        private void HandleEditBookFlyout(Book book)
        {
            OpenFlyoutPanel(new EditBookViewModel(book));
        }

        private void HandleAddBooksFlyout(IList<string> booksToAdd)
        {
            OpenFlyoutPanel(new AddNewBooksViewModel(booksToAdd));
        }

        private void ProcessEscKey()
        {
            if (IsDialogOpen && DialogControl.CanBeClosedByUser())
            {
                MessengerInstance.Send(new CloseDialogMessage());
                return;
            }

            if (IsDialogOpen && !DialogControl.CanBeClosedByUser())
            {
                return;
            }

            if (flyoutControl.IsOpen)
            {
                MessengerInstance.Send(new CloseFlyoutMessage());
                return;
            }

            MessengerInstance.Send(new GoBackMessage());
        }

        private void HandleOpenFlyout(ViewModelBase obj)
        {
            OpenFlyoutPanel(obj);
        }

        public void Dispose()
        {
            fileSystemWatcher.Dispose();
            MessengerInstance.Unregister(this);
        }
    }
}
