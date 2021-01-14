using System.IO;
using System.Text.Json;
using System.Windows;
using Valyreon.Elib.DataLayer;
using Valyreon.Elib.Wpf.Models;

namespace Valyreon.Elib.Wpf
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static UnitOfWorkFactory UnitOfWorkFactory { get; } = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);

        private void OnStartup(object sender, StartupEventArgs e)
        {

        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            //Database.Vacuum(); this slows the shutdown of application
            File.WriteAllText(ApplicationSettings.GetInstance().PropertiesPath,
                JsonSerializer.Serialize(ApplicationSettings.GetInstance()));
        }
    }
}
