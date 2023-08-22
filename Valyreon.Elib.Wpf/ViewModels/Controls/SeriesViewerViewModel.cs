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
using Valyreon.Elib.Wpf.Models;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class SeriesViewerViewModel : ViewModelBase, IViewer
    {
        private bool isResultEmpty;
        private string caption = "Series";

        public ObservableCollection<BookSeries> Series { get; set; } = new ObservableCollection<BookSeries>();

        public ICommand BackCommand => new RelayCommand(Back);

        public string Caption
        {
            get => caption;
            set => Set(() => Caption, ref caption, value);
        }

        private string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                Set(() => SearchText, ref searchText, value);
                Refresh();
            }
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
            MessengerInstance.Register<RefreshCurrentViewMessage>(this, _ =>
            {
                if (!isLoading)
                {
                    Refresh();
                }
            });
        }

        public void Clear()
        {
            Series.Clear();
        }

        private volatile bool isLoading;

        public void Refresh()
        {
            isLoading = true;
            Series.Clear();
            LoadSeries();
        }

        public ICommand GoToSeries => new RelayCommand<BookSeries>(a => Messenger.Default.Send(new SeriesSelectedMessage(a)));

        private async void LoadSeries()
        {
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var x = string.IsNullOrWhiteSpace(SearchText)
                ? await uow.SeriesRepository.GetAllAsync()
                : await uow.SeriesRepository.SearchAsync(SearchText);

            if (!x.Any())
            {
                IsResultEmpty = true;
                return;
            }

            foreach (var item in x)
            {
                Series.Add(item);
                await Task.Delay(7);
            }

            foreach (var item in x)
            {
                item.NumberOfBooks = await uow.SeriesRepository.CountBooksInSeriesAsync(item.Id);
            }

            isLoading = false;
        }

        public Func<IViewer> GetCloneFunction(Selector selector)
        {
            return () => new SeriesViewerViewModel();
        }

        public void Dispose()
        {
            MessengerInstance.Unregister(this);
        }
    }
}
