using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.Views.Controls
{
    /// <summary>
    /// Interaction logic for DashboardControl.xaml
    /// </summary>
    public partial class DashboardControl : UserControl
    {
        public DashboardControl()
        {
            InitializeComponent();
        }

        private void MoveWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Window.GetWindow(this).DragMove();
        }
    }
}
