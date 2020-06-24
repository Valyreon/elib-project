using MVVMLibrary;

namespace ElibWpf.ViewModels.Controls
{
    public class QuotesTabViewModel : ViewModelBase, ITabViewModel
    {
        private string caption = "Quotes";

        public string Caption
        {
            get => this.caption;
            set => this.Set(() => Caption, ref this.caption, value);
        }
    }
}