using DataLayer;
using Models;
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
        public static ElibContext Database = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);

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

        private void OnExit(object sender, ExitEventArgs e)
        {
            //Database.Vacuum(); this slows the shutdown of application
        }
    }
}