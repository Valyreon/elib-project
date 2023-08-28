using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm.Messaging;

namespace Valyreon.Elib.Wpf.Messages
{
    public class BookSelectedMessage : MessageBase
    {
        public BookSelectedMessage(Book book, bool isCtrlDown = false, bool isShiftDown = false)
        {
            Book = book;
            IsCtrlDown = isCtrlDown;
            IsShiftDown = isShiftDown;
        }

        public Book Book { get; }
        public bool IsCtrlDown { get; }
        public bool IsShiftDown { get; }
    }
}
