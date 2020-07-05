using MahApps.Metro.Controls;

namespace ElibWpf.Views.Windows
{
    /// <summary>
    /// Interaction logic for ReaderWindow.xaml
    /// </summary>
    public partial class ReaderWindow : MetroWindow
    {
        public ReaderWindow()
        {
            /*if (!Cef.IsInitialized)
            {
                CefSettings s = new CefSettings();
                s.CefCommandLineArgs.Add("disable-threaded-scrolling", "1");
                Cef.Initialize(s);
            }*/

            InitializeComponent();
            Browser.Focus();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Cef.Shutdown();
        }

        private void Browser_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            Browser.Focus();
        }
    }
}
