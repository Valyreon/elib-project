using System.Windows.Controls;

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

        private void ScrollViewer_PreviewMouseDown(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta/10);
            e.Handled = true;
        }
    }
}
