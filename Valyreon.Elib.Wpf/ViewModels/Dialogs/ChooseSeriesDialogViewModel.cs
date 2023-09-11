using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Interfaces;

namespace Valyreon.Elib.Wpf.ViewModels.Dialogs
{
    public class ChooseSeriesDialogViewModel : DialogViewModel
    {
        private readonly Action<BookSeries> onConfirm;
        private readonly IEnumerable<BookSeries> series;
        private string filterText;
        private BookSeries selectedItem;

        public ChooseSeriesDialogViewModel(IEnumerable<BookSeries> series, Action<BookSeries> onConfirm)
        {
            this.series = series;
            ShownSeries = new ObservableCollection<BookSeries>(series);
            this.onConfirm = onConfirm;
        }

        public ICommand CancelCommand => new RelayCommand(Close);

        public ICommand DoneCommand => new RelayCommand(Done);

        public ICommand FilterChangedCommand => new RelayCommand(FilterSeries);

        public string FilterText
        {
            get => filterText;
            set => Set(() => FilterText, ref filterText, value);
        }

        public BookSeries SelectedItem
        {
            get => selectedItem;
            set => Set(() => SelectedItem, ref selectedItem, value);
        }

        public ObservableCollection<BookSeries> ShownSeries { get; set; }

        private List<BookSeries> AllSeries { get; } = new List<BookSeries>();

        private void Done()
        {
            if (SelectedItem != null)
            {
                onConfirm(SelectedItem);
            }

            Close();
        }

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
    }
}
