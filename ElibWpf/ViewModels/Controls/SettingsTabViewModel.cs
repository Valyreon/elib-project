using GalaSoft.MvvmLight;

namespace ElibWpf.ViewModels.Controls
{
    public class SettingsTabViewModel : ViewModelBase, ITabViewModel
    {
        private string caption = "Settings";

        public string Caption
        {
            get => this.caption;
            set => this.Set(ref this.caption, value);
        }
    }
}