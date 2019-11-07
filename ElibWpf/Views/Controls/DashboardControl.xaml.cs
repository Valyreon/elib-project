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

        private void HandleLeftClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var thisWindow = Window.GetWindow(this);
                if(thisWindow.WindowState == WindowState.Maximized)
                {
                    thisWindow.WindowState = WindowState.Normal;
                }
                else if(thisWindow.WindowState == WindowState.Normal)
                {
                    thisWindow.WindowState = WindowState.Maximized;
                }

            }
            else
            {
                Window.GetWindow(this).DragMove();
            }
        }
    }
}
