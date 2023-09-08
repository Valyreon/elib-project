using Valyreon.Elib.Mvvm;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class SettingsTabViewModel : ViewModelWithValidation, ITabViewModel
    {
        public string Caption { get; set; } = "Settings";

        public ViewModelBase SettingsContent { get; }

        public SettingsTabViewModel()
        {
            //SettingsContent = new ApplicationSettingsViewModel();
        }
    }
}
