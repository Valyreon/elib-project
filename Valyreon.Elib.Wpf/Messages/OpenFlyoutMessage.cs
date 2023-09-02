using Valyreon.Elib.Mvvm;

namespace Valyreon.Elib.Wpf.Messages
{
    public class OpenFlyoutMessage
    {
        public OpenFlyoutMessage(ViewModelBase viewModel)
        {
            ViewModel = viewModel;
        }

        public ViewModelBase ViewModel { get; }
    }
}
