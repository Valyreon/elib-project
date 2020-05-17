using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DataLayer;
using Domain;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using Models;

namespace ElibWpf.ViewModels.Dialogs
{
    public class ChooseSeriesDialogViewModel : ViewModelBase
    {
        private readonly Action<BookSeries> onConfirm;
        private string filterText;

        private BookSeries selectedItem;


        public ChooseSeriesDialogViewModel(Action<BookSeries> onConfirm)
        {
            this.onConfirm = onConfirm;
        }

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        public ICommand DoneCommand => new RelayCommand(this.Done);

        public ICommand FilterChangedCommand => new RelayCommand(this.FilterSeries);

        public string FilterText
        {
            get => this.filterText;
            set => this.Set(() => this.FilterText, ref this.filterText, value);
        }

        public ICommand LoadSeriesCommand => new RelayCommand(this.LoadSeries);

        public BookSeries SelectedItem
        {
            get => this.selectedItem;
            set => this.Set(() => this.SelectedItem, ref this.selectedItem, value);
        }

        public ObservableCollection<BookSeries> ShownSeries { get; set; } = new ObservableCollection<BookSeries>();

        private List<BookSeries> AllSeries { get; } = new List<BookSeries>();

        private void FilterSeries()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //semaphore.Wait();
                this.ShownSeries.Clear();
                foreach (BookSeries a in this.AllSeries.Where(a => a.Name.ToLower().Contains(this.FilterText.ToLower())))
                {
                    this.ShownSeries.Add(a);
                }

                //semaphore.Release();
            });
        }

        private async void LoadSeries()
        {
            using ElibContext context = ApplicationSettings.CreateContext();
            var list = await context.Series.ToListAsync();
            foreach (BookSeries series in list)
            {
                this.AllSeries.Add(series);
                this.ShownSeries.Add(series);
            }
        }

        private async void Cancel()
        {
            await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext,
                await DialogCoordinator.Instance.GetCurrentDialogAsync<BaseMetroDialog>(Application.Current.MainWindow
                    .DataContext));
        }

        private async void Done()
        {
            if (this.SelectedItem != null)
            {
                await Task.Run(() => this.onConfirm(this.SelectedItem));
            }

            this.Cancel();
        }
    }
}