using Valyreon.Elib.Wpf.Interfaces;

namespace Valyreon.Elib.Wpf.ViewModels.Dialogs
{
    public class ProgressBarWithMessageDialogViewModel : DialogViewModel
    {
        private int barMaximum;
        private int barMinimum;
        private string currentMessage;
        private string title;
        private int currentBarValue;

        public int BarMaximum { get => barMaximum; set => Set(() => BarMaximum, ref barMaximum, value); }
        public int BarMinimum { get => barMinimum; set => Set(() => BarMinimum, ref barMinimum, value); }
        public string CurrentMessage { get => currentMessage; set => Set(() => CurrentMessage, ref currentMessage, value); }
        public string Title { get => title; set => Set(() => Title, ref title, value); }
        public int CurrentBarValue { get => currentBarValue; set => Set(() => CurrentBarValue, ref currentBarValue, value); }

        public override bool CanBeClosedByUser()
        {
            return false;
        }
    }
}
