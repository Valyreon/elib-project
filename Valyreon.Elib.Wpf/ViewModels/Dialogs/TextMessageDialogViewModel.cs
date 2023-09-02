using System.Windows.Input;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Interfaces;

namespace Valyreon.Elib.Wpf.ViewModels.Dialogs;

public class TextMessageDialogViewModel : DialogViewModel
{
    private string title;
    private string text;

    public TextMessageDialogViewModel(string title, string text)
    {
        Title = title;
        Text = text;
    }

    public string Title { get => title; set => Set(() => Title, ref title, value); }
    public string Text { get => text; set => Set(() => Text, ref text, value); }

    public ICommand OkCommand => new RelayCommand(Close);
}
