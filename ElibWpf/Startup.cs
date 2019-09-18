using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf
{
    public class Startup : System.Windows.Application
    {
        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main()
        {
            Console.WriteLine("Checking arguments");
            //check if app is being run in CLI mode
            string[] args = Environment.GetCommandLineArgs();
            foreach (var x in args)
            {
                Console.WriteLine(x);
            }
            if (args.Contains("-cli"))
            {
                Console.WriteLine("Starting eLIB in CLI mode.");
                // don't show the UI
                String consoleInput;
                Console.WriteLine("\nWELCOME TO ELIB COMMAND LINE.\n");
                do
                {
                    Console.Write(">> ");
                    consoleInput = Console.ReadLine();
                } while (consoleInput.ToLower() != "exit");
                if (consoleInput.ToLower() == "exit")
                    Console.WriteLine("Exiting...");
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
