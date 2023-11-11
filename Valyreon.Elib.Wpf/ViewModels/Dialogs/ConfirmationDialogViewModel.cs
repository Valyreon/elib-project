using System;
using System.Windows.Input;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Interfaces;

namespace Valyreon.Elib.Wpf.ViewModels.Dialogs
{
    public class ConfirmationDialogViewModel : DialogViewModel
    {
        private readonly Action onConfirm;
        private string noText;
        private string text;
        private string title;
        private string yesText;

        public ConfirmationDialogViewModel(string title, string text, Action onConfirm, string yesText = "YES", string noText = "NO")
        {
            Title = title;
            Text = text;
            this.onConfirm = onConfirm;
            YesText = yesText;
            NoText = noText;
        }

        public ICommand NoCommand => new RelayCommand(Close);
        public string NoText { get => noText; set => Set(() => NoText, ref noText, value); }
        public string Text { get => text; set => Set(() => Text, ref text, value); }
        public string Title { get => title; set => Set(() => Title, ref title, value); }
        public ICommand YesCommand => new RelayCommand(HandleConfirm);
        public string YesText { get => yesText; set => Set(() => YesText, ref yesText, value); }

        private void HandleConfirm()
        {
            Close();
            onConfirm();
        }
    }
}
