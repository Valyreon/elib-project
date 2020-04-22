using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using DataLayer;
using Models;
using Newtonsoft.Json;

namespace ElibWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ElibContext Database = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);

        public static Selector Selector { get; private set; }

        private Dispatcher splashScreenDispacher;

        private void OnStartup(object sender, StartupEventArgs e)
        {
            Selector = new Selector(App.Database);
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
            Logger.Log("APP_EXIT", "");
            //Database.Vacuum(); this slows the shutdown of application
            File.WriteAllText(ApplicationSettings.GetInstance().PropertiesPath, JsonConvert.SerializeObject(ApplicationSettings.GetInstance(), Formatting.Indented));
        }
    }
}