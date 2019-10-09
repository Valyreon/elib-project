using System;
using System.Linq;

namespace ElibWpf
{
    public class Startup : System.Windows.Application
    {
        /// <summary>
        /// Application Entry Point.
        /// </summary>
        ///
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main()
        {
            Console.WriteLine("Initializing local DB");
            //DatabaseContext database = DatabaseContext.GetInstance();
            Console.WriteLine("Checking arguments");
            // Check if app is being run in CLI mode
            string[] args = Environment.GetCommandLineArgs();
            foreach (string x in args)
            {
                Console.WriteLine(x);
            }
            if (args.Contains("-cli")) // Don't show the GUI
            {
                //CliExecutor cliExecutor = new CliExecutor();
                //cliExecutor.Execute();
            }
            else
            {
                ElibWpf.App app = new ElibWpf.App();
                app.InitializeComponent();
                app.Run();
            }
        }

    }
}
