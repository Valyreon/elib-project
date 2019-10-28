using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.ViewModels.Controls
{
    public class QuotesTabViewModel : ViewModelBase, IPageViewModel
    {
        private string _caption = "Quotes";

        public string Caption
        {
            get => _caption;
            set => Set(ref _caption, value);
        }
    }
}
