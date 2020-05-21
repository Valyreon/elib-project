using MVVMLibrary.Messaging;

namespace MVVMLibrary
{
    public class ViewModelBase : ObservableObject
    {
        protected IMessenger MessengerInstance { get => Messenger.Default; }
    }
}