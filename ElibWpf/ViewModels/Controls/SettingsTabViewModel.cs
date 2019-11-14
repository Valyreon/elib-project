using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
