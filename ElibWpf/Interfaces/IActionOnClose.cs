using System;

namespace ElibWpf.Interfaces
{
    public interface IActionOnClose
    {
        void SetActionOnClose(Action action);
        void Close();
    }
}
