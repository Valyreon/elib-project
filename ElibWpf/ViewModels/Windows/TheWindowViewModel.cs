using ElibWpf.Messages;
using ElibWpf.ViewModels.Controls;
using ElibWpf.ViewModels.Flyouts;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using System.Collections.ObjectModel;
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

            Tabs = new ObservableCollection<ITabViewModel>
            {
                new BooksTabViewModel(),
                new QuotesTabViewModel(),
                new SettingsTabViewModel()
            };
            SelectedTab = Tabs[0];
        }

        public ICommand CloseDetailsCommand { get => new RelayCommand(() => { IsBookDetailsFlyoutOpen = false; FlyoutControl = null; }); }

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