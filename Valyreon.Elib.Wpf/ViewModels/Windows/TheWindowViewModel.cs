using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows.Input;
using Valyreon.Elib.DataLayer;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Interfaces;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.Themes.CustomComponents.Controls;
using Valyreon.Elib.Wpf.ViewModels.Controls;
using Valyreon.Elib.Wpf.ViewModels.Flyouts;
using Timer = System.Timers.Timer;

namespace Valyreon.Elib.Wpf.ViewModels.Windows
{
    public class TheWindowViewModel : ViewModelBase, IDisposable
    {
        private static readonly TimeSpan refreshPause = new TimeSpan(0, 0, 0, 0, 500);
        private static DateTime lastRefresh = DateTime.MinValue;
        private readonly ApplicationProperties applicationProperties = ApplicationData.GetProperties();
        private readonly Queue<ShowNotificationMessage> messages = new();
        private readonly Timer notificationTimer = new();
        private readonly IUnitOfWorkFactory unitOfWorkFactory = new UnitOfWorkFactory(ApplicationData.DatabasePath);
        private ShowNotificationMessage currentNotificationMessage;
        private DialogViewModel dialogControl;
        private ElibFileSystemWatcher fileSystemWatcher;
        private IFlyoutPanel flyoutControl;
        private bool isDialogOpen;
        private bool isGlobalLoaderOpen;
        private ITabViewModel selectedTab;

        public TheWindowViewModel()
        {
            notificationTimer.Interval = 3000;
            notificationTimer.Elapsed += HandleNextNotification;

            fileSystemWatcher = new(applicationProperties, unitOfWorkFactory);

            MessengerInstance.Register<AppSettingsChangedMessage>(this, _ =>
            {
                fileSystemWatcher.Dispose();
                fileSystemWatcher = new ElibFileSystemWatcher(applicationProperties, unitOfWorkFactory);
            });
            MessengerInstance.Register<OpenFlyoutMessage>(this, m => HandleOpenFlyout(m.ViewModel));
            MessengerInstance.Register<OpenBookDetailsFlyoutMessage>(this, m => HandleOpenFlyout(new BookDetailsViewModel(m.Book, applicationProperties, unitOfWorkFactory)));
            MessengerInstance.Register<ShowNotificationMessage>(this, HandleShowNotification);
            MessengerInstance.Register(this, (ShowDialogMessage m) =>
            {
                if (!IsDialogOpen)
                {
                    DialogControl = m.ViewModel;
                    IsDialogOpen = true;
                }
            });
            MessengerInstance.Register(this, (CloseDialogMessage _) => IsDialogOpen = false);
            MessengerInstance.Register(this, (CloseFlyoutMessage _) => CloseFlyoutPanel());
            MessengerInstance.Register(this, (SetGlobalLoaderMessage m) => IsGlobalLoaderOpen = m.IsVisible);

            Tabs = new ObservableCollection<ITabViewModel>
            {
                new BooksTabViewModel(applicationProperties, unitOfWorkFactory),
                new QuotesTabViewModel(),
                new ApplicationSettingsViewModel(applicationProperties, unitOfWorkFactory)
            };
            SelectedTab = Tabs[0];

            FlyoutControl = new FlyoutPanel();
        }

        public ICommand CloseFlyoutCommand => new RelayCommand(() => FlyoutControl = null);

        public ShowNotificationMessage CurrentNotificationMessage
        {
            get => currentNotificationMessage;
            set => Set(() => CurrentNotificationMessage, ref currentNotificationMessage, value);
        }

        public DialogViewModel DialogControl
        {
            get => dialogControl;
            set => Set(() => DialogControl, ref dialogControl, value);
        }

        public ICommand EscKeyCommand => new RelayCommand(ProcessEscKey);

        public IFlyoutPanel FlyoutControl
        {
            get => flyoutControl;
            set => Set(() => FlyoutControl, ref flyoutControl, value);
        }

        public bool IsDialogOpen
        {
            get => isDialogOpen;
            set => Set(() => IsDialogOpen, ref isDialogOpen, value);
        }

        public bool IsGlobalLoaderOpen
        {
            get => isGlobalLoaderOpen;
            set => Set(() => IsGlobalLoaderOpen, ref isGlobalLoaderOpen, value);
        }

        public ICommand LeftKeyCommand => new RelayCommand(() => MessengerInstance.Send(new KeyPressedMessage(Key.Left)));

        public ICommand NextNotificationCommand => new RelayCommand(() => HandleNextNotification());

        public ICommand RefreshViewCommand => new RelayCommand(HandleRefreshView);

        public ICommand RightKeyCommand => new RelayCommand(() => MessengerInstance.Send(new KeyPressedMessage(Key.Right)));

        public ITabViewModel SelectedTab
        {
            get => selectedTab;
            set => Set(() => SelectedTab, ref selectedTab, value);
        }

        public ObservableCollection<ITabViewModel> Tabs { get; set; }

        public void CloseFlyoutPanel()
        {
            flyoutControl.IsOpen = false;
            flyoutControl.ContentControl = null;
        }

        public void Dispose()
        {
            fileSystemWatcher.Dispose();
            MessengerInstance.Unregister(this);
        }

        public void HandleRefreshView()
        {
            if (DateTime.Now - lastRefresh > refreshPause)
            {
                MessengerInstance.Send(new RefreshCurrentViewMessage());
                lastRefresh = DateTime.Now;
            }
        }

        public void OpenFlyoutPanel(object content)
        {
            flyoutControl.ContentControl = content;
            flyoutControl.IsOpen = true;
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

        private void HandleOpenFlyout(ViewModelBase obj)
        {
            OpenFlyoutPanel(obj);
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
    }
}
