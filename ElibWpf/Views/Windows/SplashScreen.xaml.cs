using System.Windows;
using System.Windows.Interop;

namespace ElibWpf.Views.Windows
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper winHelper = new WindowInteropHelper(this);
        }
    }
}