using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;
using ElibWpf.Windows;

namespace ElibWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Dispatcher splashScreenDispacher;

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var splashScreenThread = new Thread(() =>
                {
                    var splashScreen = new Windows.SplashScreen();
                    splashScreenDispacher = splashScreen.Dispatcher;
                    splashScreenDispacher.ShutdownStarted += (o, args) => splashScreen.Close();
                    splashScreen.Show();
                    Dispatcher.Run();
                });
        }
    }
}