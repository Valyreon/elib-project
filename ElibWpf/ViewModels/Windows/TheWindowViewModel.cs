using ElibWpf.Messages;
using ElibWpf.ViewModels.Controls;
using ElibWpf.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Windows
{
    public class TheWindowViewModel : ViewModelBase
    {
        private object currentControl;

        private bool isBookDetailsFlyoutOpen;
        public bool IsBookDetailsFlyoutOpen
        {
            get => isBookDetailsFlyoutOpen;
            set => Set(ref isBookDetailsFlyoutOpen, value);
        }

        public ICommand CloseDetailsCommand { get => new RelayCommand(() => { IsBookDetailsFlyoutOpen = false; }); }

        public TheWindowViewModel()
        {
            CurrentControl = new DashboardViewModel();
            MessengerInstance.Register<ShowBookDetailsMessage>(this, this.HandleBookFlyout);
        }

        private void HandleBookFlyout(ShowBookDetailsMessage obj)
        {
            IsBookDetailsFlyoutOpen = true;
        }

        public object CurrentControl
        {
            get => this.currentControl;
            set => Set("CurrentControl", ref currentControl, value);
        }

        private object flyoutControl;
        public object FlyoutControl
        {
            get => this.flyoutControl;
            set => Set("FyloutControl", ref flyoutControl, value);
        }
    }
}