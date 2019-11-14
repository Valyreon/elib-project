using GalaSoft.MvvmLight;

namespace ElibWpf.ViewModels.Controls
{
    public class SettingsTabViewModel : ViewModelBase, ITabViewModel
    {
        private string _caption = "Settings";

        public string Caption
        {
            get => _caption;
            set => Set(ref _caption, value);
        }
    }
}