using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Valyreon.Elib.DataLayer;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Mvvm.Messaging;
using Valyreon.Elib.Wpf.Messages;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class SeriesViewerViewModel : ViewModelBase, IViewer
    {
        private bool isResultEmpty;
        private string caption;

        public ObservableCollection<BookSeries> Series { get; set; } = new ObservableCollection<BookSeries>();

        public ICommand LoadCommand => new RelayCommand(LoadMore);
        public ICommand BackCommand => new RelayCommand(Back);

        public string Caption
        {
            get => caption;
            set => Set(() => Caption, ref caption, value);
        }

        public FilterParameters Filter => null;
        private Action backAction;
        public Action Back
        {
            get => backAction;
            set
            {
                Set(() => Back, ref backAction, value);
                RaisePropertyChanged(() => IsBackEnabled);
            }
        }

        public bool IsBackEnabled => Back != null;

        public bool IsResultEmpty
        {
            get => isResultEmpty;
            set => Set(() => IsResultEmpty, ref isResultEmpty, value);
        }

        public SeriesViewerViewModel()
        {
            MessengerInstance.Register<RefreshCurrentViewMessage>(this, _ => Refresh());
        }

        public void Clear()
        {
            Series.Clear();
        }

        public void Refresh()
        {
            Series.Clear();
            LoadMore();
        }

        public Task<IViewer> Search(SearchParameters searchOptions)
        {
            throw new NotImplementedException();
        }

        public ICommand GoToSeries => new RelayCommand<BookSeries>(a => Messenger.Default.Send(new SeriesSelectedMessage(a)));

        private async void LoadMore()
        {
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var x = await uow.SeriesRepository.GetAllAsync();

            if (!x.Any())
            {
                IsResultEmpty = true;
                return;
            }

            foreach (var item in x)
            {
                item.NumberOfBooks = await uow.SeriesRepository.CountBooksInSeriesAsync(item.Id);
                Series.Add(item);
                await Task.Delay(6);
            }
        }
    }
}
