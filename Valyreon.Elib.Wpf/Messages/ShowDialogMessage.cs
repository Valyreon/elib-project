using Valyreon.Elib.Wpf.Interfaces;

namespace Valyreon.Elib.Wpf.Messages
{
    public class ShowDialogMessage
    {
        public ShowDialogMessage(DialogViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public DialogViewModel ViewModel { get; }
    }
}
