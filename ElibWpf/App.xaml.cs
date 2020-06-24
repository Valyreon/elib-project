using Models;
using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace ElibWpf
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private Dispatcher splashScreenDispacher;

        private void OnStartup(object sender, StartupEventArgs e)
        {
            /*var splashScreenThread = new Thread(() =>
                {
                    var splashScreen = new Views.Windows.SplashScreen();
                    splashScreenDispacher = splashScreen.Dispatcher;
                    splashScreenDispacher.ShutdownStarted += (o, args) => splashScreen.Close();
                    splashScreen.Show();
                    Dispatcher.Run();
                });*/
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            Logger.Log("APP_EXIT", "");
            //Database.Vacuum(); this slows the shutdown of application
            File.WriteAllText(ApplicationSettings.GetInstance().PropertiesPath,
                JsonConvert.SerializeObject(ApplicationSettings.GetInstance(), Formatting.Indented));
        }
    }
}