using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.Views.Controls
{
    /// <summary>
    ///     Interaction logic for BooksControl.xaml
    /// </summary>
    public partial class BooksTabControl : UserControl
    {
        public BooksTabControl()
        {
            this.InitializeComponent();
        }

        private void SearchOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            switch (this.SearchContentRow.Height.Value)
            {
                case 60:
                    //SearchContentRow.Height = new System.Windows.GridLength(150);
                    this.SearchOptionsHeightAnimation.From = new GridLength(60);
                    this.SearchOptionsHeightAnimation.To = new GridLength(150);
                    this.AngleDoubleAnimation.To = -180;
                    break;

                case 150:
                    //SearchContentRow.Height = new System.Windows.GridLength(60);
                    this.SearchOptionsHeightAnimation.From = new GridLength(150);
                    this.SearchOptionsHeightAnimation.To = new GridLength(60);
                    this.AngleDoubleAnimation.To = 0;
                    break;

                default:
                    return;
            }
        }
    }
}