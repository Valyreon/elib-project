using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElibWpf.Resources
{
    public partial class DashboardTabControlStyles : ResourceDictionary
    {
        public void HandleLeftClick(object sender, MouseButtonEventArgs e)
        {
            Grid grid = sender as Grid;
            Point mousePosition = e.GetPosition(grid);

            if (!(mousePosition.X >= 0) || !(mousePosition.Y >= 0) || !(mousePosition.X <= grid.ActualWidth) ||
                !(mousePosition.Y <= grid.ActualHeight))
            {
                return;
            }

            Window thisWindow = Window.GetWindow(grid);
            if (e.ClickCount == 2)
            {
                if (thisWindow != null)
                {
                    thisWindow.WindowState = thisWindow.WindowState switch
                    {
                        WindowState.Maximized => WindowState.Normal,
                        WindowState.Normal => WindowState.Maximized,
                        _ => thisWindow.WindowState
                    };
                }
            }
            else
            {
                thisWindow?.DragMove();
            }
        }
    }
}