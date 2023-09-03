using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Animation;
using Valyreon.Elib.Wpf.ViewModels.Windows;

namespace Valyreon.Elib.Wpf.Views.Windows
{
    /// <summary>
    ///     Interaction logic for TheWindow.xaml
    /// </summary>
    public partial class TheWindow : Window
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

        private void DialogGrid_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (bool)e.NewValue;
            if (newValue)
            {
                DialogGrid.Visibility = Visibility.Visible;
                var sb = FindResource("DialogGridFadeInStoryboard") as Storyboard;
                sb?.Begin(DialogGrid, false);
                //sb.Completed += (s, e) => DialogContentControl.Focus();
            }
            else
            {
                var sb = FindResource("DialogGridFadeOutStoryboard") as Storyboard;
                sb?.Begin(DialogGrid, false);
                sb.Completed += (s, e) => DialogGrid.Visibility = Visibility.Collapsed;

                var binding = BindingOperations.GetBinding(DialogContentControl, ContentProperty);

                sb.Completed += (s, e) => DialogContentControl.Content = null;
            }
        }
    }
}
