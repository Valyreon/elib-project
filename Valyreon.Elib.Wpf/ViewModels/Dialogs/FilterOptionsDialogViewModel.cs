using System;
using System.Windows.Input;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.BindingItems;
using Valyreon.Elib.Wpf.Interfaces;

namespace Valyreon.Elib.Wpf.ViewModels.Dialogs
{
    public class FilterOptionsDialogViewModel : DialogViewModel
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

        public ICommand CancelCommand => new RelayCommand(Close);

        public ICommand ApplyCommand => new RelayCommand(Apply);

        private void Apply()
        {
            Close();
            onConfirm(Options);
        }
    }
}
