using Domain;
using ElibWpf.Models;
using MahApps.Metro.Controls.Dialogs;
using MVVMLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
				//semaphore.Wait();
				ShownSeries.Clear();
				foreach(var a in AllSeries.Where(a => a.Name.ToLower().Contains(FilterText.ToLower())))
				{
					ShownSeries.Add(a);
				}

				//semaphore.Release();
			});
		}

		private void LoadSeries()
		{
			using var uow = ApplicationSettings.CreateUnitOfWork();
			var list = uow.SeriesRepository.All().ToList();
			foreach(var series in list)
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

		private async void Done()
		{
			if(SelectedItem != null)
			{
				await Task.Run(() => onConfirm(SelectedItem));
			}

			Cancel();
		}
	}
}
