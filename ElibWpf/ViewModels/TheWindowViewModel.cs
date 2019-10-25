using ElibWpf.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Windows.Input;

namespace ElibWpf.ViewModels
{
    public class TheWindowViewModel : ViewModelBase
    {
        private readonly Stack<object> viewModelHistory = new Stack<object>();
        private object currentControl;

        public TheWindowViewModel()
        {
            CurrentControl = new DashboardViewModel();
        }

        public object CurrentControl
        {
            get
            {
                return this.currentControl;
            }

            internal set
            {
                viewModelHistory.Push(value);
                Set("CurrentControl", ref currentControl, value);
            }
        }

        public ICommand GoBack { get => new RelayCommand(GoToPreviousViewModel); }

        public void GoToPreviousViewModel()
        {
            if (viewModelHistory.Count > 1)
                viewModelHistory.Pop();
            this.CurrentControl = viewModelHistory.Peek();
        }
    }
}