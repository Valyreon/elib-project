using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Valyreon.Elib.Wpf.Views.Controls
{
    /// <summary>
    /// Interaction logic for EditBookFormControl.xaml
    /// </summary>
    public partial class EditBookFormControl : UserControl
    {
        public EditBookFormControl()
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
