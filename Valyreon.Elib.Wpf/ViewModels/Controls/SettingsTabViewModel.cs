using Valyreon.Elib.Mvvm;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class SettingsTabViewModel : ViewModelWithValidation, ITabViewModel
    {
        public SettingsTabViewModel()
        {
            //SettingsContent = new ApplicationSettingsViewModel();
        }

        public string Caption { get; set; } = "Settings";

        public ViewModelBase SettingsContent { get; }
    }
}
