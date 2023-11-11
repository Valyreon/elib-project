using Valyreon.Elib.Wpf.Interfaces;

namespace Valyreon.Elib.Wpf.ViewModels.Dialogs
{
    public class ProgressBarWithMessageDialogViewModel : DialogViewModel
    {
        private int barMaximum;
        private int barMinimum;
        private int currentBarValue;
        private string currentMessage;
        private string title;
        public int BarMaximum { get => barMaximum; set => Set(() => BarMaximum, ref barMaximum, value); }
        public int BarMinimum { get => barMinimum; set => Set(() => BarMinimum, ref barMinimum, value); }
        public int CurrentBarValue { get => currentBarValue; set => Set(() => CurrentBarValue, ref currentBarValue, value); }
        public string CurrentMessage { get => currentMessage; set => Set(() => CurrentMessage, ref currentMessage, value); }
        public string Title { get => title; set => Set(() => Title, ref title, value); }

        public override bool CanBeClosedByUser()
        {
            return false;
        }
    }
}
