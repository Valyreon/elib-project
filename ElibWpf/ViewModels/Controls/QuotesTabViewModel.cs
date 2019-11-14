using GalaSoft.MvvmLight;

namespace ElibWpf.ViewModels.Controls
{
    public class QuotesTabViewModel : ViewModelBase, ITabViewModel
    {
        private string _caption = "Quotes";

        public string Caption
        {
            get => _caption;
            set => Set(ref _caption, value);
        }
    }
}