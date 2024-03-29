using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ViewModels.Windows;
using Valyreon.Elib.Wpf.Views.Windows;

namespace Valyreon.Elib.Wpf
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if TEST
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32", SetLastError = true)]
        public static extern void FreeConsole();
#endif

        private static Mutex _mutex = null;
        private bool _isMutexOwner = false;

        private void OnStartup(object sender, StartupEventArgs e)
        {
            _mutex = new Mutex(true, GetType().Namespace.ToString(), out _isMutexOwner);

            if (!_isMutexOwner)
            {
                Current.Shutdown();
            }

            ApplicationData.InitializeAppData();

            SetupExceptionHandling();

#if TEST
            AllocConsole();
#endif
            //Current.Resources["AccentColorBrush"] = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffaacc");
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
#if TEST
            FreeConsole();
#endif
            foreach (Window window in Current.Windows)
            {
                if (window is TheWindow)
                {
                    var viewModel = window.DataContext as TheWindowViewModel;
                    window.DataContext = null;
                    viewModel.Dispose();
                }
            }

            if (_isMutexOwner)
            {
                _mutex.ReleaseMutex();
            }
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

            stringBuilder.AppendLine(message)
                .AppendLine()
                .AppendLine(exception.ToString());

            var logPath = Path.Combine(ApplicationData.LogFolderPath, $"{timestamp:yyyyMMddTHHmmssfff}_{exception.GetType().Name}.txt");
            File.WriteAllText(logPath, stringBuilder.ToString());

#if TEST
            Console.WriteLine(exception.ToString());
            Console.WriteLine("++++++++++++++++++++++++");
#endif
        }
    }
}
