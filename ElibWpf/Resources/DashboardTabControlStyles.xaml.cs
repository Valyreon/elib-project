using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.Resources
{
    public partial class DashboardTabControlStyles : ResourceDictionary
    {
        public void HandleLeftClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var grid = sender as Grid;
            var mousePosition = e.GetPosition(grid);

            if (mousePosition.X >= 0 && mousePosition.Y >= 0 && mousePosition.X <= grid.ActualWidth && mousePosition.Y <= grid.ActualHeight)
            {
                var thisWindow = Window.GetWindow(grid);
                if (e.ClickCount == 2)
                {
                    if (thisWindow.WindowState == WindowState.Maximized)
                    {
                        thisWindow.WindowState = WindowState.Normal;
                    }
                    else if (thisWindow.WindowState == WindowState.Normal)
                    {
                        thisWindow.WindowState = WindowState.Maximized;
                    }
                }
                else
                {
                    thisWindow.DragMove();
                }
            }
        }
    }
}