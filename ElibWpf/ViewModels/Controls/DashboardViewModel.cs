using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace ElibWpf.ViewModels.Controls
{
    public class DashboardViewModel : ViewModelBase
    {
        private ObservableCollection<IPageViewModel> _pages = new ObservableCollection<IPageViewModel>();
        public ObservableCollection<IPageViewModel> Pages
        {
            get => _pages;
            private set { Set(() => Pages, ref _pages, value); }
        }

        public DashboardViewModel()
        {
            var books = new BooksTabViewModel();
            var quotes = new QuotesTabViewModel();
            var settings = new SettingsTabViewModel();
            Pages = new ObservableCollection<IPageViewModel>
            {
                books,
                quotes,
                settings
            };
        }
    }
}
