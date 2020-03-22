using ElibWpf.ViewModels.Controls;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ElibWpf.Views.Controls
{
    /// <summary>
    /// Interaction logic for BooksControl.xaml
    /// </summary>
    public partial class BooksTabControl : UserControl
    {
        public BooksTabControl()
        {
            InitializeComponent();
        }

        private void SearchOptionsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            
            switch (SearchContentRow.Height.Value)
            {
                case 60:
                    SearchContentRow.Height = new System.Windows.GridLength(150);
                    AngleDoubleAnimation.To = -180;
                    break;
                case 150:
                    SearchContentRow.Height = new System.Windows.GridLength(60);
                    AngleDoubleAnimation.To = 0;
                    break;
                default:
                    return;
            }
        }
    }
}