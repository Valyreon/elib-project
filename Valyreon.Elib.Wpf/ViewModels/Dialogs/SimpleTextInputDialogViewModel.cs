using System;
using System.Windows.Input;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Interfaces;

namespace Valyreon.Elib.Wpf.ViewModels.Dialogs
{
    internal class SimpleTextInputDialogViewModel : DialogViewModel
    {
        private readonly Action<string> onConfirm;
        private string title;
        private string text;
        private string inputText;

        public SimpleTextInputDialogViewModel(string title, string text, Action<string> onConfirm)
        {
            Title = title;
            Text = text;
            this.onConfirm = onConfirm;
        }

        public ICommand CancelCommand => new RelayCommand(Close);

        public ICommand ConfirmCommand => new RelayCommand(HandleSubmit);

        private void HandleSubmit()
        {
            onConfirm(InputText);
            Close();
        }

        public string Title { get => title; set => Set(() => Title, ref title, value); }
        public string Text { get => text; set => Set(() => Text, ref text, value); }
        public string InputText { get => inputText; set => Set(() => InputText, ref inputText, value); }
    }
}
