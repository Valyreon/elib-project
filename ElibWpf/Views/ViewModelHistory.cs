using System.Collections.Generic;

namespace FileEncryptorWpf.Views
{
    /// <summary>
    /// Defines the <see cref="ViewModelHistory" /> interface. This interface should be implemented by every window which can change content to some <see cref="UserControl"/>.
    /// </summary>
    public abstract class ViewModelHistory
    {
        protected readonly Stack<object> viewModelHistory = new Stack<object>();

        protected abstract void SetCurrentControl(object obj);

        public void GoToPreviousViewModel()
        {
            viewModelHistory.Pop();
            SetCurrentControl(viewModelHistory.Peek());
        }

        public void GoToViewModel(object x)
        {
            viewModelHistory.Push(x);
            SetCurrentControl(x);
        }
    }
}