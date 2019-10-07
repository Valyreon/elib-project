using System.ComponentModel;

namespace ElibWpf.ViewModels
{
    public class MainWindowViewModel : WindowControlHistory, INotifyPropertyChanged
    {
        private object currentControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralViewModel"/> class.
        /// </summary>
        /// <param name="thisWindow">Window in which all the UserControls are to be shown in.</param>
        public MainWindowViewModel()
        {
            //this.GoToControl(some viewmodel); TODO: initialize first control here
        }

        public object CurrentControl
        {
            get
            {
                return this.currentControl;
            }

            internal set
            {
                this.currentControl = value;
                this.RaisePropertyChangedEvent("CurrentControl");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void SetCurrentControl(object obj)
        {
            this.CurrentControl = obj;
        }
    }
}
