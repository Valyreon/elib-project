using System.Windows.Controls;
using System.Windows.Input;

namespace Valyreon.Elib.Wpf.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for SimpleTextInputDialog.xaml
    /// </summary>
    public partial class SimpleTextInputDialog : UserControl
    {
        public SimpleTextInputDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, System.EventArgs e)
        {
            Keyboard.Focus(InputTextBox);
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Keyboard.Focus(InputTextBox);
        }
    }
}
