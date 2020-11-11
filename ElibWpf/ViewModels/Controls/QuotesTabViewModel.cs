using MVVMLibrary;

namespace ElibWpf.ViewModels.Controls
{
	public class QuotesTabViewModel : ViewModelBase, ITabViewModel
	{
		private string caption = "Quotes";

		public string Caption
		{
			get => caption;
			set => Set(() => Caption, ref caption, value);
		}
	}
}
