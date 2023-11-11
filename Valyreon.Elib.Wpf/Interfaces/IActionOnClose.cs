using System;

namespace Valyreon.Elib.Wpf.Interfaces
{
    public interface IActionOnClose
    {
        void Close();

        void SetActionOnClose(Action action);
    }
}
