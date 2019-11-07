using ElibWpf.ViewModels.Controls;
using ElibWpf.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Windows
{
    public class TheWindowViewModel : ViewModelBase
    {
        private object currentControl;

        public TheWindowViewModel()
        {
            CurrentControl = new DashboardViewModel();
        }

        public object CurrentControl
        {
            get => this.currentControl;
            set => Set("CurrentControl", ref currentControl, value);
        }
    }
}