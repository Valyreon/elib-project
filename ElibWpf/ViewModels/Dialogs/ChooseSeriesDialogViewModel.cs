using Domain;
using System;

namespace ElibWpf.ViewModels.Dialogs
{
    public class ChooseSeriesDialogViewModel
    {
        private Action<BookSeries> onConfirm;

        public ChooseSeriesDialogViewModel(Action<BookSeries> onConfirm)
        {
            this.onConfirm = onConfirm;
        }
    }
}
