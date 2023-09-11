using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.ViewModels;

namespace Valyreon.Elib.Wpf.Interfaces
{
    public abstract class DialogViewModel : ViewModelWithValidation
    {
        public virtual bool CanBeClosedByUser()
        {
            return true;
        }

        public virtual void Close()
        {
            MessengerInstance.Send(new CloseDialogMessage());
        }
    }
}
