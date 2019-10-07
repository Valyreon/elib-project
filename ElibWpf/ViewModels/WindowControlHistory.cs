using System.Collections.Generic;

namespace ElibWpf.ViewModels
{
    /// <summary>
    /// Defines the <see cref="WindowControlHistory" /> interface. This interface should be implemented by every window which can change content to some <see cref="UserControl"/>.
    /// </summary>
    public abstract class WindowControlHistory
    {
        protected readonly Stack<object> viewModelHistory = new Stack<object>();

        protected abstract void SetCurrentControl(object obj);

        public void GoToPreviousControl()
        {
            if (viewModelHistory.Count > 1)
                viewModelHistory.Pop();
            SetCurrentControl(viewModelHistory.Peek());
        }

        public void GoToControl(object x)
        {
            viewModelHistory.Push(x);
            SetCurrentControl(x);
        }
    }
}