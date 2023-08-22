using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Valyreon.Elib.Wpf.Views.Flyouts
{
	/// <summary>
	///     Interaction logic for EditBookControl.xaml
	/// </summary>
	public partial class EditBookControl : UserControl
	{
		public EditBookControl()
		{
			InitializeComponent();
		}


        private static readonly Regex digitsOnly = new(@"^\d*\.?\d?$");
        private void SeriesNumberField_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var isMatch = digitsOnly.IsMatch(e.Text);
            e.Handled = !isMatch;
        }
    }
}
