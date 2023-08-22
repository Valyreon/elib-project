using System.Threading.Tasks;
using System;
using System.Windows;
using Valyreon.Elib.DataLayer;
using Valyreon.Elib.Wpf.Models;
using System.Text;
using System.IO;

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

            SetupExceptionHandling();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {

        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            var message = $"Unhandled exception ({source})";
            try
            {
                var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = string.Format("Unhandled exception in {0} v{1}", assemblyName.Name, assemblyName.Version);
            }
            catch (Exception ex)
            {
                LogException(ex, "Exception in LogUnhandledException");
            }
            finally
            {
                LogException(exception, message);
            }
        }

        private static void LogException(Exception exception, string message)
        {
            var timestamp = DateTime.UtcNow;
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(message);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(exception.ToString());

            var logPath = Path.Combine(ApplicationData.LogFolderPath, $"{timestamp:yyyyMMddTHHmmss}_{exception.GetType().Name}.txt");
            File.WriteAllText(logPath, stringBuilder.ToString());
        }
    }
}
