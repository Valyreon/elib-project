using Valyreon.Elib.Mvvm;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class SettingsTabViewModel : ViewModelBase, ITabViewModel
    {
        private string caption = "Settings";

        public string Caption
        {
            get => caption;
            set => Set(() => Caption, ref caption, value);
        }
    }
}
