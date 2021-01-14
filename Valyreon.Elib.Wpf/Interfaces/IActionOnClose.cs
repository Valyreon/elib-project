using System;

namespace Valyreon.Elib.Wpf.Interfaces
{
    public interface IActionOnClose
    {
        void SetActionOnClose(Action action);
        void Close();
    }
}
