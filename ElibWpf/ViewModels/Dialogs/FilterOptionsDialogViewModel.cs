using ElibWpf.BindingItems;
using MahApps.Metro.Controls.Dialogs;
using MVVMLibrary;
using System;
using System.Windows;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Dialogs
{
	public class FilterOptionsDialogViewModel : ViewModelBase
	{
		public FilterOptions Options { get; }

		private int orderSelectedIndex;
		private readonly Action<FilterOptions> onConfirm;

		public int OrderSelectedIndex
		{
			get => orderSelectedIndex;
			set
			{
				Set(() => OrderSelectedIndex, ref orderSelectedIndex, value);
				Options.Ascending = orderSelectedIndex != 0;
			}
		}

		public FilterOptionsDialogViewModel(FilterOptions options, Action<FilterOptions> onConfirm)
		{
			Options = (FilterOptions)options.Clone();
			this.onConfirm = onConfirm;
			OrderSelectedIndex = Options.Ascending ? 1 : 0;
		}

		public ICommand CancelCommand => new RelayCommand(Cancel);

		public ICommand ApplyCommand => new RelayCommand(Apply);

		private async void Apply()
		{
			await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext,
				await DialogCoordinator.Instance.GetCurrentDialogAsync<BaseMetroDialog>(Application.Current.MainWindow
					.DataContext));
			onConfirm(Options);
		}

		private async void Cancel()
		{
			await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext,
				await DialogCoordinator.Instance.GetCurrentDialogAsync<BaseMetroDialog>(Application.Current.MainWindow
					.DataContext));
		}
	}
}
