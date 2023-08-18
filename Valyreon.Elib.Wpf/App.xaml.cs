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
        public static UnitOfWorkFactory UnitOfWorkFactory { get; } = new UnitOfWorkFactory(ApplicationData.DatabasePath);

        private void OnStartup(object sender, StartupEventArgs e)
        {
            ApplicationData.InitializeAppData();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {

        }
    }
}
