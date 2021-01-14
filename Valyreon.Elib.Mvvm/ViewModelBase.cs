using Valyreon.Elib.Mvvm.Messaging;

namespace Valyreon.Elib.Mvvm
{
    public class ViewModelBase : ObservableObject
    {
        protected IMessenger MessengerInstance => Messenger.Default;
    }
}
