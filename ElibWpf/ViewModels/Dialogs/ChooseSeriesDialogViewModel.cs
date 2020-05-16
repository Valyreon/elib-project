using DataLayer;
using Domain;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Dialogs
{
    public class ChooseSeriesDialogViewModel : ViewModelBase
    {
        private Action<BookSeries> onConfirm;
        private List<BookSeries> allSeries { get; } = new List<BookSeries>();


        public ChooseSeriesDialogViewModel(Action<BookSeries> onConfirm)
        {
            this.onConfirm = onConfirm;
        }

        private BookSeries selectedItem;
        public BookSeries SelectedItem
        {
            get => selectedItem;
            set => Set(() => SelectedItem, ref selectedItem, value);
        }

        private string filterText;
        public string FilterText
        {
            get => filterText;
            set => Set(() => FilterText, ref filterText, value);
        }

        public ICommand FilterChangedCommand { get => new RelayCommand(this.FilterSeries); }

        private void FilterSeries()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                //semaphore.Wait();
                ShownSeries.Clear();
                foreach (BookSeries a in allSeries.Where(a => a.Name.ToLower().Contains(FilterText.ToLower())))
                {
                    ShownSeries.Add(a);
                }
                //semaphore.Release();
            });
        }

        public ObservableCollection<BookSeries> ShownSeries { get; set; } = new ObservableCollection<BookSeries>();

        public ICommand LoadSeriesCommand { get => new RelayCommand(this.LoadSeries); }

        private async void LoadSeries()
        {
            using ElibContext context = ApplicationSettings.CreateContext();
            var list = await context.Series.ToListAsync();
            foreach (var series in list)
            {
                allSeries.Add(series);
                ShownSeries.Add(series);
            }
        }

        public ICommand CancelCommand { get => new RelayCommand(this.Cancel); }

        private async void Cancel()
        {
            await DialogCoordinator.Instance.HideMetroDialogAsync(App.Current.MainWindow.DataContext, await DialogCoordinator.Instance.GetCurrentDialogAsync<BaseMetroDialog>(App.Current.MainWindow.DataContext));
        }

        public ICommand DoneCommand { get => new RelayCommand(this.Done); }

        private async void Done()
        {
            if(SelectedItem != null)
                await Task.Run(() => onConfirm(SelectedItem));
            Cancel();
        }
    }
}
