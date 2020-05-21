using ElibWpf.Messages;
using MahApps.Metro.Controls.Dialogs;
using Models.Options;
using MVVMLibrary;
using System;
using System.Windows;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Dialogs
{
    public class FilterOptionsDialogViewModel : ViewModelBase
    {
        public FilterOptions Options { get; }
        private FilterOptions oldOptions { get; }

        int orderSelectedIndex;
        private readonly Action<FilterOptions> onConfirm;

        public int OrderSelectedIndex
        {
            get => orderSelectedIndex;
            set
            {
                Set(() => OrderSelectedIndex, ref orderSelectedIndex, value);
                Options.Ascending = orderSelectedIndex == 0 ? false : true;
            }
        }

        public FilterOptionsDialogViewModel(FilterOptions options, Action<FilterOptions> onConfirm)
        {
            this.Options = (FilterOptions)options.Clone();
            this.oldOptions = options;
            this.onConfirm = onConfirm;
            OrderSelectedIndex = Options.Ascending ? 1 : 0;
        }

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        public ICommand ApplyCommand => new RelayCommand(this.Apply);

        private async void Apply()
        {
            onConfirm(Options);
            await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext,
                await DialogCoordinator.Instance.GetCurrentDialogAsync<BaseMetroDialog>(Application.Current.MainWindow
                    .DataContext));
            this.MessengerInstance.Send(new RefreshCurrentViewMessage());
        }

        private async void Cancel()
        {
            await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext,
                await DialogCoordinator.Instance.GetCurrentDialogAsync<BaseMetroDialog>(Application.Current.MainWindow
                    .DataContext));
        }
    }
}
