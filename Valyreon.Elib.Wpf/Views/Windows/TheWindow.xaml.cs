using System.Windows.Media.Animation;
using MahApps.Metro.Controls;
using Valyreon.Elib.Wpf.ViewModels.Windows;

namespace Valyreon.Elib.Wpf.Views.Windows
{
    /// <summary>
    ///     Interaction logic for TheWindow.xaml
    /// </summary>
    public partial class TheWindow : MetroWindow
    {
        public TheWindow()
        {
            InitializeComponent();
            DataContext = new TheWindowViewModel();
        }

        private void NotificationGrid_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var sb = FindResource("ShowNotificationStoryboard") as Storyboard;
            sb?.Begin(NotificationGrid, false);
        }
    }
}
