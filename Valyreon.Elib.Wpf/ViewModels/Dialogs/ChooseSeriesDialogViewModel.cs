using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;

namespace Valyreon.Elib.Wpf.ViewModels.Dialogs
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

        public ICommand CancelCommand => new RelayCommand(Cancel);

        public ICommand DoneCommand => new RelayCommand(Done);

        public ICommand FilterChangedCommand => new RelayCommand(FilterSeries);

        public string FilterText
        {
            get => filterText;
            set => Set(() => FilterText, ref filterText, value);
        }

        public ICommand LoadSeriesCommand => new RelayCommand(LoadSeries);

        public BookSeries SelectedItem
        {
            get => selectedItem;
            set => Set(() => SelectedItem, ref selectedItem, value);
        }

        public ObservableCollection<BookSeries> ShownSeries { get; set; } = new ObservableCollection<BookSeries>();

        private List<BookSeries> AllSeries { get; } = new List<BookSeries>();

        private void FilterSeries()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShownSeries.Clear();
                foreach (var a in AllSeries.Where(a => a.Name.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    ShownSeries.Add(a);
                }
            });
        }

        private async void LoadSeries()
        {
            IEnumerable<BookSeries> list = null;

            using (var uow = await App.UnitOfWorkFactory.CreateAsync())
            {
                list = await uow.SeriesRepository.GetAllAsync();
            }

            foreach (var series in list)
            {
                AllSeries.Add(series);
                ShownSeries.Add(series);
            }
        }

        private async void Cancel()
        {
            await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext,
                await DialogCoordinator.Instance.GetCurrentDialogAsync<BaseMetroDialog>(Application.Current.MainWindow
                    .DataContext));
        }

        private void Done()
        {
            if (SelectedItem != null)
            {
                onConfirm(SelectedItem);
            }

            Cancel();
        }
    }
}
