using System.Threading;
using System.Windows;
using System.Windows.Threading;

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
                    var splashScreen = new Views.Windows.SplashScreen();
                    splashScreenDispacher = splashScreen.Dispatcher;
                    splashScreenDispacher.ShutdownStarted += (o, args) => splashScreen.Close();
                    splashScreen.Show();
                    Dispatcher.Run();
                });
        }
    }
}